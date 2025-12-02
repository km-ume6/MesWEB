using ClosedXML.Excel;
using MesWEB.Shared.Models;
using MesWEB.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace MesWEB.ExcelCompare.Components.Pages;

public partial class ExcelCompare : IAsyncDisposable
{
    private class UploadedBook
    {
        public string FileName { get; set; } = string.Empty;
        public XLWorkbook Workbook { get; set; } = null!;
    }

    private List<CellMapping> cellMappings = new();
    private List<CellComparisonResult>? comparisonResults;
    private List<UploadedBook> uploadedBooks = new();
    private bool isLoading = false;
    private string errorMessage = string.Empty;
    private List<string> tempFiles = new(); // CSV ダウンロード用の一時ファイルリスト

    // テンプレート関連
    private List<CellMappingTemplate> savedTemplates = new();
    private int selectedTemplateId = 0;
    private string newTemplateName = string.Empty;
    private string newTemplateDescription = string.Empty;
    private string templateMessage = string.Empty;

    // ドラッグ状態
    private int dragSourceIndex = -1;
    private int dragTargetIndex = -1;

    // ヘルプモーダル
    private bool showHelp = false;

    // Compact view: 展開状態を管理
    private HashSet<int> expandedMappings = new();

    // 一括変換モーダル
    private bool showBulkRename = false;
    private string sourceBookName = string.Empty;
    private string targetBookName = string.Empty;
    private string sourceSheetName = string.Empty;
    private string targetSheetName = string.Empty;
    private int renameMode = 1; // 1: ブック名のみ, 2: シート名のみ, 3: 両方

    [Inject] private ILogger<ExcelCompare> Logger { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private DotNetObjectReference<ExcelCompare>? dotNetRef;

    protected override async Task OnInitializedAsync()
    {
        // 初期メッセージを設定
        templateMessage = "テンプレートを読み込んでいます...";

        // テンプレート読み込みを遅延実行（UIブロックを防ぐ）
        await Task.Yield();

        try
        {
            await LoadSavedTemplates();

            // 読み込み成功後にUIを更新
            if (savedTemplates != null && savedTemplates.Count > 0)
            {
                templateMessage = $"{savedTemplates.Count}件のテンプレートを読み込みました";
            }
            else
            {
                templateMessage = "保存されたテンプレートはありません";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "OnInitializedAsync: テンプレート読み込みエラー");
            templateMessage = $"エラー: {ex.Message}";

            // エラー時も空のリストを確保してUIを表示
            if (savedTemplates == null)
            {
                savedTemplates = new List<CellMappingTemplate>();
            }
        }
        finally
        {
            // UIを強制的に更新
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            try
            {
                await JS.InvokeVoidAsync("initMappingSortable", dotNetRef);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Sortable init failed (graceful fallback)");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        dotNetRef?.Dispose();
    }

    // JS called when Sortable finished
    [JSInvokable]
    public void OnSortableEnd(int oldIndex, int newIndex)
    {
        try
        {
            if (oldIndex < 0 || oldIndex >= cellMappings.Count) return;
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= cellMappings.Count) newIndex = cellMappings.Count - 1;

            var item = cellMappings[oldIndex];
            cellMappings.RemoveAt(oldIndex);

            // adjust destination if necessary when removing earlier index
            if (newIndex > oldIndex) newIndex--;

            cellMappings.Insert(newIndex, item);

            // update expandedMappings indices
            var newExpanded = new HashSet<int>();
            foreach (var idx in expandedMappings)
            {
                if (idx == oldIndex)
                {
                    newExpanded.Add(newIndex);
                }
                else
                {
                    var adjusted = idx;
                    if (oldIndex < idx && idx <= newIndex) adjusted = idx - 1;
                    if (newIndex <= idx && idx < oldIndex) adjusted = idx + 1;
                    newExpanded.Add(adjusted);
                }
            }
            expandedMappings = newExpanded;

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "OnSortableEnd error");
        }
    }

    // Compact view helpers
    private void ToggleMappingExpand(int index)
    {
        if (expandedMappings.Contains(index))
            expandedMappings.Remove(index);
        else
            expandedMappings.Add(index);

        StateHasChanged();
    }

    private bool IsMappingExpanded(int index) => expandedMappings.Contains(index);

    // expand/collapse all
    private void ExpandAll()
    {
        expandedMappings.Clear();
        for (int i = 0; i < cellMappings.Count; i++)
        {
            expandedMappings.Add(i);
        }
        StateHasChanged();
    }

    private void CollapseAll()
    {
        expandedMappings.Clear();
        StateHasChanged();
    }

    // scroll to top/bottom
    private async Task ScrollToTop()
    {
        try
        {
            await JS.InvokeVoidAsync("scrollToElement", "mapping-list", "start");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "ScrollToTop failed");
        }
    }

    private async Task ScrollToBottom()
    {
        try
        {
            await JS.InvokeVoidAsync("scrollToElement", "mapping-list", "end");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "ScrollToBottom failed");
        }
    }

    // existing drag handlers kept for fallback
    private void OnDragStart(DragEventArgs e, int index)
    {
        dragSourceIndex = index;
        dragTargetIndex = -1;
        StateHasChanged();
    }

    private void OnDragOver(DragEventArgs e, int index)
    {
        dragTargetIndex = index;
        StateHasChanged();
    }

    private void OnDrop(DragEventArgs e, int targetIndex)
    {
        // older fallback - keep for non-JS scenario
        try
        {
            if (dragSourceIndex < 0 || dragSourceIndex >= cellMappings.Count) return;

            var sourceIndex = dragSourceIndex;
            var destIndex = targetIndex;

            if (sourceIndex == destIndex) return;

            var item = cellMappings[sourceIndex];
            cellMappings.RemoveAt(sourceIndex);

            if (destIndex > sourceIndex)
            {
                destIndex--;
            }

            if (destIndex < 0) destIndex = 0;
            if (destIndex > cellMappings.Count) destIndex = cellMappings.Count;

            cellMappings.Insert(destIndex, item);

            dragSourceIndex = -1;
            dragTargetIndex = -1;

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "OnDropエラー");
        }
    }

    private void OnDragEnd(DragEventArgs e)
    {
        dragSourceIndex = -1;
        dragTargetIndex = -1;
        StateHasChanged();
    }

    // ファイル名からブックインデックスを更新（内部的にのみ使用）
    private void UpdateBookIndexFromFileName(int mappingIndex, int bookNumber)
    {
        if (mappingIndex < 0 || mappingIndex >= cellMappings.Count)
            return;

        var mapping = cellMappings[mappingIndex];

        if (bookNumber == 1)
        {
            mapping.Book1Index = uploadedBooks.FindIndex(b => b.FileName == mapping.Book1FileName);
        }
        else if (bookNumber == 2)
        {
            mapping.Book2Index = uploadedBooks.FindIndex(b => b.FileName == mapping.Book2FileName);
        }

        StateHasChanged();
    }

    // 全マッピングのブックインデックスを更新
    private void UpdateAllBookIndices()
    {
        foreach (var mapping in cellMappings)
        {
            mapping.Book1Index = uploadedBooks.FindIndex(b => b.FileName == mapping.Book1FileName);
            mapping.Book2Index = uploadedBooks.FindIndex(b => b.FileName == mapping.Book2FileName);
        }
    }

    private bool CanCompare()
    {
        if (cellMappings.Count == 0) return false;

        return cellMappings.All(m =>
              !string.IsNullOrWhiteSpace(m.Book1FileName) &&
           !string.IsNullOrWhiteSpace(m.Sheet1Name) &&
              !string.IsNullOrWhiteSpace(m.Sheet1Cell) &&
         !string.IsNullOrWhiteSpace(m.Book2FileName) &&
              !string.IsNullOrWhiteSpace(m.Sheet2Name) &&
     !string.IsNullOrWhiteSpace(m.Sheet2Cell) &&
     m.Book1Index >= 0 &&
       m.Book2Index >= 0
          );
    }

    private void AddMapping()
    {
        var newIndex = cellMappings.Count;
        cellMappings.Add(new CellMapping
        {
            ItemName = $"項目{cellMappings.Count + 1}",
            Sheet1Cell = "A1",
            Sheet2Cell = "A1",
            Sheet1Formula = FormulaType.None,
            Sheet2Formula = FormulaType.None,
            DecimalPlaces = 2,
            Tolerance = 0.0
        });

        // 新しい項目をデフォルトで展開状態にする
        expandedMappings.Add(newIndex);

        StateHasChanged();
    }

    private void RemoveMapping(int index)
    {
        if (index >= 0 && index < cellMappings.Count)
        {
            cellMappings.RemoveAt(index);
            // 展開状態をクリア
            expandedMappings.Remove(index);
            // adjust any stored expanded indices greater than removed index
            var toAdjust = expandedMappings.Where(i => i > index).ToList();
            if (toAdjust.Any())
            {
                foreach (var i in toAdjust)
                {
                    expandedMappings.Remove(i);
                    expandedMappings.Add(i - 1);
                }
            }
        }

        StateHasChanged();
    }

    private void ClearMappings()
    {
        cellMappings.Clear();
        comparisonResults = null;
        expandedMappings.Clear();

        StateHasChanged();
    }

    private void RemoveUploadedFile(string fileName)
    {
        var book = uploadedBooks.FirstOrDefault(b => b.FileName == fileName);
        if (book != null)
        {
            book.Workbook?.Dispose();
            uploadedBooks.Remove(book);

            templateMessage = $"ファイル「{fileName}」を削除しました（残り{uploadedBooks.Count}個）";

            // 全マッピングのインデックスを再計算
            UpdateAllBookIndices();
        }
    }

    private void ClearUploadedFiles()
    {
        foreach (var book in uploadedBooks)
        {
            book.Workbook?.Dispose();
        }
        uploadedBooks.Clear();
        comparisonResults = null;

        // インデックスをリセット（ファイル名は保持）
        foreach (var mapping in cellMappings)
        {
            mapping.Book1Index = -1;
            mapping.Book2Index = -1;
        }

        templateMessage = "全てのファイルをクリアしました";
    }

    private string QuoteCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        // カンマ、ダブルクォーテーションをエスケープ
        value = value.Replace("\"", "\"\"");

        // ダブルクォーテーションで囲む
        return $"\"{value}\"";
    }

    private void ShowHelp()
    {
        showHelp = true;
        StateHasChanged();
    }

    private void HideHelp()
    {
        showHelp = false;
        StateHasChanged();
    }

    // Bulk rename methods
    private void ShowBulkRename()
    {
        showBulkRename = true;
        sourceBookName = string.Empty;
        targetBookName = string.Empty;
        sourceSheetName = string.Empty;
        targetSheetName = string.Empty;
        renameMode = 1;
        StateHasChanged();
    }

    private void HideBulkRename()
    {
        showBulkRename = false;
        StateHasChanged();
    }

    private void ApplyBulkRename()
    {
        try
        {
            if (cellMappings.Count == 0)
            {
                templateMessage = "変換する項目がありません";
                HideBulkRename();
                return;
            }

            int changedCount = 0;

            // 新しいリストを作成してBlazorに変更を確実に検知させる
            var updatedMappings = new List<CellMapping>();

            foreach (var mapping in cellMappings)
            {
                bool changed = false;
                var updatedMapping = mapping; // 参照をコピー

                // ブック1の変換 (mode: 1 = ブック1のみ, 5 = 両方)
                if ((renameMode == 1 || renameMode == 5) && !string.IsNullOrEmpty(sourceBookName) && !string.IsNullOrEmpty(targetBookName))
                {
                    if (updatedMapping.Book1FileName == sourceBookName)
                    {
                        updatedMapping.Book1FileName = targetBookName;
                        changed = true;
                    }
                }

                // ブック2の変換 (mode: 2 = ブック2のみ, 5 = 両方)
                if ((renameMode == 2 || renameMode == 5) && !string.IsNullOrEmpty(sourceBookName) && !string.IsNullOrEmpty(targetBookName))
                {
                    if (updatedMapping.Book2FileName == sourceBookName)
                    {
                        updatedMapping.Book2FileName = targetBookName;
                        changed = true;
                    }
                }

                // ブック1のシート名の変換 (mode: 3 = ブック1のシートのみ)
                if (renameMode == 3 && !string.IsNullOrEmpty(sourceSheetName) && !string.IsNullOrEmpty(targetSheetName))
                {
                    if (updatedMapping.Sheet1Name == sourceSheetName)
                    {
                        updatedMapping.Sheet1Name = targetSheetName;
                        changed = true;
                    }
                }

                // ブック2のシート名の変換 (mode: 4 = ブック2のシートのみ)
                if (renameMode == 4 && !string.IsNullOrEmpty(sourceSheetName) && !string.IsNullOrEmpty(targetSheetName))
                {
                    if (updatedMapping.Sheet2Name == sourceSheetName)
                    {
                        updatedMapping.Sheet2Name = targetSheetName;
                        changed = true;
                    }
                }

                if (changed)
                {
                    changedCount++;
                }

                updatedMappings.Add(updatedMapping);
            }

            // リスト全体を置き換えてBlazorに変更を通知
            cellMappings = updatedMappings;

            // ブックインデックスを再計算
            UpdateAllBookIndices();

            var modeText = renameMode switch
            {
                1 => "ブック1のファイル名",
                2 => "ブック2のファイル名",
                3 => "ブック1のシート名",
                4 => "ブック2のシート名",
                5 => "ブック1とブック2のファイル名",
                _ => "項目"
            };

            templateMessage = $"{modeText}を{changedCount}項目一括変換しました";
            Logger.LogInformation($"一括変換完了 (mode={renameMode}): {changedCount}項目");

            HideBulkRename();
            
            // 強制的に再レンダリング
            InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            templateMessage = $"一括変換エラー: {ex.Message}";
            Logger.LogError(ex, "一括変換エラー");
            HideBulkRename();
        }
    }

    // Get unique book names from current mappings (with mode support)
    private List<string> GetUniqueBookNames(int mode = 0)
    {
        var bookNames = new HashSet<string>();
        foreach (var mapping in cellMappings)
        {
            // mode: 0=all, 1=Book1 only, 2=Book2 only, 5=both
            if ((mode == 0 || mode == 1 || mode == 5) && !string.IsNullOrEmpty(mapping.Book1FileName))
                bookNames.Add(mapping.Book1FileName);
            if ((mode == 0 || mode == 2 || mode == 5) && !string.IsNullOrEmpty(mapping.Book2FileName))
                bookNames.Add(mapping.Book2FileName);
        }
        return bookNames.OrderBy(n => n).ToList();
    }

    // Get unique sheet names from current mappings (with mode support)
    private List<string> GetUniqueSheetNames(int mode = 0)
    {
        var sheetNames = new HashSet<string>();
        foreach (var mapping in cellMappings)
        {
            // mode: 0=all, 3=Book1's sheet only, 4=Book2's sheet only
            if ((mode == 0 || mode == 3) && !string.IsNullOrEmpty(mapping.Sheet1Name))
                sheetNames.Add(mapping.Sheet1Name);
            if ((mode == 0 || mode == 4) && !string.IsNullOrEmpty(mapping.Sheet2Name))
                sheetNames.Add(mapping.Sheet2Name);
        }
        return sheetNames.OrderBy(n => n).ToList();
    }
}
