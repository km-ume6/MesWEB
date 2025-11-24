using ClosedXML.Excel;
using MesWEB.Shared.Models;
using MesWEB.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace MesWEB.ExcelCompare.Components.Pages;

public partial class ExcelCompare
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

    // ヘルプモーダル
    private bool showHelp = false;

    [Inject] private ILogger<ExcelCompare> Logger { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

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

        StateHasChanged();
    }

    private void RemoveMapping(int index)
    {
        if (index >= 0 && index < cellMappings.Count)
        {
            cellMappings.RemoveAt(index);
        }

        StateHasChanged();
    }

    private void ClearMappings()
    {
        cellMappings.Clear();
        comparisonResults = null;

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
}
