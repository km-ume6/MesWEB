using MesWEB.Shared.Data;
using MesWEB.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace MesWEB.ExcelCompare.Components.Pages;

public partial class ExcelCompare
{
    private async Task LoadSavedTemplates()
    {
        try
        {
            // タイムアウト付きでクエリ実行（10秒に延長）
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            Logger.LogInformation("データベースからテンプレートとラベルを読み込んでいます...");

            // DbContextFactoryから新しいインスタンスを取得
            await using var db = await DbFactory.CreateDbContextAsync(cts.Token);

            // ラベルを読み込み
            savedLabels = await db.CellMappingLabels
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.LabelName)
                .AsNoTracking()
                .ToListAsync(cts.Token);

            Logger.LogInformation($"ラベル読み込み完了: {savedLabels.Count}件");

            savedTemplates = await db.CellMappingTemplates
                .Include(t => t.MappingItems)
                .OrderBy(t => t.TemplateName)
                .AsNoTracking()
                .ToListAsync(cts.Token);

            Logger.LogInformation($"テンプレート読み込み完了: {savedTemplates.Count}件");

            // デバッグ: テンプレート名をログ出力
            foreach (var template in savedTemplates)
            {
                Logger.LogInformation($"  - {template.TemplateName} (ID: {template.TemplateId}, LabelID: {template.LabelId}, Items: {template.MappingItems?.Count ?? 0})");
            }
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, "テンプレート読み込みがタイムアウトしました（10秒）");
            if (savedTemplates == null)
            {
                savedTemplates = new List<CellMappingTemplate>();
            }
            if (savedLabels == null)
            {
                savedLabels = new List<CellMappingLabel>();
            }
            templateMessage = "テンプレートの読み込みがタイムアウトしました。データベース接続を確認してください。";
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "テンプレート読み込みエラー: {Message}", ex.Message);
            Logger.LogError(ex, "InnerException: {InnerMessage}", ex.InnerException?.Message);

            if (savedTemplates == null)
            {
                savedTemplates = new List<CellMappingTemplate>();
            }
            if (savedLabels == null)
            {
                savedLabels = new List<CellMappingLabel>();
            }
            templateMessage = $"テンプレートの読み込みに失敗しました: {ex.Message}";
            throw;
        }
    }

    private async Task SaveTemplate()
    {
        try
        {
            var inputName = newTemplateName?.Trim();
            
            // 入力された名前が既存テンプレートと一致するか確認
            var existingTemplate = !string.IsNullOrWhiteSpace(inputName)
                ? savedTemplates.FirstOrDefault(t => t.TemplateName.Equals(inputName, StringComparison.OrdinalIgnoreCase))
                : null;

            // 上書き条件:
            // 1. テンプレートが選択されていて、名前が未入力または既存名と一致
            // 2. 入力された名前が既存テンプレートと一致
            if (existingTemplate != null)
            {
                // 既存テンプレート名を入力した場合、そのテンプレートを上書き
                selectedTemplateId = existingTemplate.TemplateId;
                await UpdateTemplate();
                return;
            }
            else if (selectedTemplateId > 0 && string.IsNullOrWhiteSpace(inputName))
            {
                // テンプレート選択中で名前未入力の場合、選択中のテンプレートを上書き
                await UpdateTemplate();
                return;
            }

            // 新規保存
            if (string.IsNullOrWhiteSpace(inputName))
            {
                templateMessage = "テンプレート名を入力してください";
                return;
            }

            var template = new CellMappingTemplate
            {
                TemplateName = inputName,
                Description = newTemplateDescription?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var sortOrder = 0;
            foreach (var mapping in cellMappings)
            {
                // ファイル名とシート名で保存
                var sheet1Name = $"{mapping.Book1FileName}:{mapping.Sheet1Name}";
                var sheet2Name = $"{mapping.Book2FileName}:{mapping.Sheet2Name}";

                Logger.LogInformation($"保存: {mapping.ItemName} -> Sheet1={sheet1Name}, Sheet2={sheet2Name}");

                template.MappingItems.Add(new CellMappingItem
                {
                    SortOrder = sortOrder++,
                    ItemName = mapping.ItemName,
                    Sheet1Name = sheet1Name,
                    Sheet1Cell = mapping.Sheet1Cell,
                    Sheet1Formula = (int)mapping.Sheet1Formula,
                    Sheet2Name = sheet2Name,
                    Sheet2Cell = mapping.Sheet2Cell,
                    Sheet2Formula = (int)mapping.Sheet2Formula,
                    DecimalPlaces = mapping.DecimalPlaces,
                    Tolerance = mapping.Tolerance
                });
            }

            await using var db = await DbFactory.CreateDbContextAsync();
            db.CellMappingTemplates.Add(template);
            await db.SaveChangesAsync();

            Logger.LogInformation($"テンプレート「{inputName}」を新規保存しました（ID: {template.TemplateId}, Items: {template.MappingItems.Count}）");
            templateMessage = $"テンプレート「{inputName}」を保存しました（{template.MappingItems.Count}項目）";
            
            // 新規保存後は選択状態をクリア
            selectedTemplateId = 0;
            newTemplateName = string.Empty;
            newTemplateDescription = string.Empty;

            await LoadSavedTemplates();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            templateMessage = $"保存エラー: {ex.Message}";
            Logger.LogError(ex, "テンプレート保存エラー: {Message}, InnerException: {Inner}", ex.Message, ex.InnerException?.Message);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task UpdateTemplate()
    {
        try
        {
            if (selectedTemplateId == 0)
            {
                templateMessage = "テンプレートを選択してください";
                return;
            }

            // 名前が未入力の場合は既存名を使用
            var templateName = string.IsNullOrWhiteSpace(newTemplateName)
              ? savedTemplates.FirstOrDefault(t => t.TemplateId == selectedTemplateId)?.TemplateName
                  : newTemplateName.Trim();

            if (string.IsNullOrWhiteSpace(templateName))
            {
                templateMessage = "テンプレート名を入力してください";
                return;
            }

            await using var db = await DbFactory.CreateDbContextAsync();

            // 既存テンプレートを取得（トラッキング有効）
            var template = await db.CellMappingTemplates
      .Include(t => t.MappingItems)
          .FirstOrDefaultAsync(t => t.TemplateId == selectedTemplateId);

            if (template == null)
            {
                templateMessage = "選択したテンプレートが見つかりません";
                Logger.LogWarning($"テンプレートID {selectedTemplateId} が見つかりませんでした");
                return;
            }

            Logger.LogInformation($"テンプレート「{template.TemplateName}」（ID: {selectedTemplateId}）を上書き更新します。既存アイテム数: {template.MappingItems.Count}");

            template.TemplateName = templateName;
            template.Description = newTemplateDescription?.Trim();
            template.UpdatedAt = DateTime.UtcNow;

            // 既存のアイテムを完全に削除（RemoveRangeで確実に削除）
            if (template.MappingItems.Any())
            {
                db.CellMappingItems.RemoveRange(template.MappingItems);
                Logger.LogInformation($"{template.MappingItems.Count} 件の既存アイテムを削除キューに追加しました");
            }

            // 新しいマッピングを追加
            var sortOrder = 0;
            foreach (var mapping in cellMappings)
            {
                var sheet1Name = $"{mapping.Book1FileName}:{mapping.Sheet1Name}";
                var sheet2Name = $"{mapping.Book2FileName}:{mapping.Sheet2Name}";

                var newItem = new CellMappingItem
                {
                    TemplateId = template.TemplateId,
                    SortOrder = sortOrder++,
                    ItemName = mapping.ItemName,
                    Sheet1Name = sheet1Name,
                    Sheet1Cell = mapping.Sheet1Cell,
                    Sheet1Formula = (int)mapping.Sheet1Formula,
                    Sheet2Name = sheet2Name,
                    Sheet2Cell = mapping.Sheet2Cell,
                    Sheet2Formula = (int)mapping.Sheet2Formula,
                    DecimalPlaces = mapping.DecimalPlaces,
                    Tolerance = mapping.Tolerance
                };

                db.CellMappingItems.Add(newItem);
                Logger.LogInformation($"新規アイテムを追加: {mapping.ItemName} -> Sheet1={sheet1Name}, Sheet2={sheet2Name}");
            }

            // 変更を保存
            var changes = await db.SaveChangesAsync();
            Logger.LogInformation($"テンプレート更新完了: {changes} 件の変更を保存しました");

            templateMessage = $"テンプレート「{templateName}」を上書き更新しました（{cellMappings.Count}項目）";
            newTemplateName = string.Empty;
            newTemplateDescription = string.Empty;

            await LoadSavedTemplates();
            await InvokeAsync(StateHasChanged);
        }
        catch (DbUpdateException dbEx)
        {
            templateMessage = $"データベース更新エラー: {dbEx.InnerException?.Message ?? dbEx.Message}";
            Logger.LogError(dbEx, "テンプレート更新時のDB例外: {Message}, InnerException: {Inner}", dbEx.Message, dbEx.InnerException?.Message);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            templateMessage = $"更新エラー: {ex.Message}";
            Logger.LogError(ex, "テンプレート更新エラー: {Message}, InnerException: {Inner}", ex.Message, ex.InnerException?.Message);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadTemplate()
    {
        try
        {
            if (selectedTemplateId == 0)
            {
                templateMessage = "テンプレートを選択してください";
                return;
            }

            await using var db = await DbFactory.CreateDbContextAsync();
            var template = await db.CellMappingTemplates
            .Include(t => t.MappingItems)
       .FirstOrDefaultAsync(t => t.TemplateId == selectedTemplateId);

            if (template == null)
            {
                templateMessage = "選択したテンプレートが見つかりません";
                return;
            }

            cellMappings.Clear();
            expandedMappings.Clear(); // 展開状態をクリア

            var itemIndex = 0;
            foreach (var item in template.MappingItems.OrderBy(i => i.SortOrder))
            {
                var sheet1Parts = item.Sheet1Name?.Split(':') ?? Array.Empty<string>();
                var sheet2Parts = item.Sheet2Name?.Split(':') ?? Array.Empty<string>();

                var mapping = new CellMapping
                {
                    ItemName = item.ItemName ?? "",
                    Book1FileName = sheet1Parts.Length > 0 ? sheet1Parts[0] : "",
                    Sheet1Name = sheet1Parts.Length > 1 ? sheet1Parts[1] : "",
                    Sheet1Cell = item.Sheet1Cell ?? "",
                    Sheet1Formula = (FormulaType)item.Sheet1Formula,
                    Book2FileName = sheet2Parts.Length > 0 ? sheet2Parts[0] : "",
                    Sheet2Name = sheet2Parts.Length > 1 ? sheet2Parts[1] : "",
                    Sheet2Cell = item.Sheet2Cell ?? "",
                    Sheet2Formula = (FormulaType)item.Sheet2Formula,
                    DecimalPlaces = item.DecimalPlaces,
                    Tolerance = item.Tolerance
                };

                cellMappings.Add(mapping);
                expandedMappings.Add(itemIndex); // デフォルトで展開状態にする
                itemIndex++;
            }

            UpdateAllBookIndices();

            newTemplateName = template.TemplateName;
            newTemplateDescription = template.Description ?? "";
            templateMessage = $"テンプレート「{template.TemplateName}」を読み込みました（{cellMappings.Count}項目）";

            StateHasChanged();
        }
        catch (Exception ex)
        {
            templateMessage = $"読み込みエラー: {ex.Message}";
            Logger.LogError(ex, "テンプレート読み込みエラー");
        }
    }

    private async Task DeleteTemplate()
    {
        try
        {
            if (selectedTemplateId == 0)
            {
                templateMessage = "テンプレートを選択してください";
                return;
            }

            var templateName = savedTemplates.FirstOrDefault(t => t.TemplateId == selectedTemplateId)?.TemplateName ?? $"ID:{selectedTemplateId}";
            var confirmed = await JS.InvokeAsync<bool>("confirm", new object[] { $"テンプレート「{templateName}」を削除しますか？" });
            if (!confirmed)
            {
                return;
            }

            await using var db = await DbFactory.CreateDbContextAsync();
            var template = await db.CellMappingTemplates.FindAsync(selectedTemplateId);
            if (template != null)
            {
                db.CellMappingTemplates.Remove(template);
                await db.SaveChangesAsync();
                Logger.LogInformation($"テンプレート「{templateName}」（ID: {selectedTemplateId}）を削除しました");
            }

            templateMessage = $"テンプレート「{templateName}」を削除しました";
            selectedTemplateId = 0;
            newTemplateName = string.Empty;
            newTemplateDescription = string.Empty;

            await LoadSavedTemplates();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            templateMessage = $"削除エラー: {ex.Message}";
            Logger.LogError(ex, "テンプレート削除エラー");
            await InvokeAsync(StateHasChanged);
        }
    }
}
