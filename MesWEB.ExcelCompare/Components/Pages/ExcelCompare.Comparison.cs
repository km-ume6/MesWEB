using ClosedXML.Excel;
using MesWEB.Shared.Models;
using static MesWEB.Shared.Models.FormulaType;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace MesWEB.ExcelCompare.Components.Pages;

public partial class ExcelCompare
{
    private async Task CompareFiles()
    {
        isLoading = true;
        errorMessage = string.Empty;
        comparisonResults = new List<CellComparisonResult>();

        await InvokeAsync(StateHasChanged);

        try
        {
            foreach (var mapping in cellMappings)
            {
                var book1Index = uploadedBooks.FindIndex(b => b.FileName == mapping.Book1FileName);
                var book2Index = uploadedBooks.FindIndex(b => b.FileName == mapping.Book2FileName);

                if (book1Index < 0 || book2Index < 0)
                {
                    errorMessage = $"項目「{mapping.ItemName}」: ファイルが見つかりません";
                    continue;
                }

                var book1 = uploadedBooks[book1Index];
                var book2 = uploadedBooks[book2Index];

                var sheet1 = book1.Workbook.Worksheets.FirstOrDefault(s => s.Name == mapping.Sheet1Name);
                var sheet2 = book2.Workbook.Worksheets.FirstOrDefault(s => s.Name == mapping.Sheet2Name);

                if (sheet1 == null || sheet2 == null)
                {
                    errorMessage = $"項目「{mapping.ItemName}」: シートが見つかりません";
                    continue;
                }

                string value1Str;
                string value2Str;

                try
                {
                    value1Str = GetCellOrRangeValue(sheet1, mapping.Sheet1Cell, mapping.Sheet1Formula, mapping.DecimalPlaces);
                    value2Str = GetCellOrRangeValue(sheet2, mapping.Sheet2Cell, mapping.Sheet2Formula, mapping.DecimalPlaces);
                }
                catch (Exception ex)
                {
                    errorMessage = $"項目「{mapping.ItemName}」: セル値取得エラー - {ex.Message}";
                    Logger.LogError(ex, "セル値取得エラー: {Item}", mapping.ItemName);
                    continue;
                }

                // 数値比較時の許容誤差を考慮
                bool isMatch = CompareValues(value1Str, value2Str, mapping.Tolerance);
                string message = isMatch ? "" : $"値が異なります（許容誤差: ±{mapping.Tolerance}）";

                var result = new CellComparisonResult
                {
                    ItemName = mapping.ItemName,
                    Book1Name = book1.FileName,
                    Sheet1Cell = mapping.Sheet1Cell,
                    Sheet1Formula = mapping.Sheet1Formula.ToString(),
                    Sheet1Value = value1Str,
                    Book2Name = book2.FileName,
                    Sheet2Cell = mapping.Sheet2Cell,
                    Sheet2Formula = mapping.Sheet2Formula.ToString(),
                    Sheet2Value = value2Str,
                    IsMatch = isMatch,
                    Message = message
                };

                comparisonResults.Add(result);
            }

            Logger.LogInformation($"比較完了: {comparisonResults.Count}項目");
        }
        catch (Exception ex)
        {
            errorMessage = $"比較処理でエラーが発生しました: {ex.Message}";
            Logger.LogError(ex, "CompareFiles error");
        }
        finally
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool CompareValues(string value1, string value2, double tolerance)
    {
        // 両方とも数値に変換できる場合は数値比較
        if (double.TryParse(value1, out var num1) && double.TryParse(value2, out var num2))
        {
            if (tolerance > 0)
            {
                return Math.Abs(num1 - num2) <= tolerance;
            }
            return num1 == num2;
        }

        // 文字列として比較
        return value1 == value2;
    }

    private string GetCellOrRangeValue(IXLWorksheet sheet, string cellAddress, FormulaType formula, int decimalPlaces)
    {
        try
        {
            if (cellAddress.Contains(":"))
            {
                // 範囲指定の場合
                var range = sheet.Range(cellAddress);
                var values = new List<double>();

                foreach (var cell in range.Cells())
                {
                    if (cell.Value.IsNumber)
                    {
                        values.Add(cell.Value.GetNumber());
                    }
                    else if (cell.Value.IsText)
                    {
                        var textValue = cell.Value.GetText();
                        if (double.TryParse(textValue, out var numValue))
                        {
                            values.Add(numValue);
                        }
                    }
                }

                if (values.Count == 0)
                {
                    return "";
                }

                double result = formula switch
                {
                    FormulaType.Max => values.Max(),
                    FormulaType.Min => values.Min(),
                    FormulaType.Average => values.Average(),
                    _ => 0
                };

                return Math.Round(result, decimalPlaces).ToString($"F{decimalPlaces}");
            }
            else
            {
                // 単一セル
                var cell = sheet.Cell(cellAddress);

                if (cell.Value.IsBlank)
                {
                    return "";
                }
                else if (cell.Value.IsNumber)
                {
                    var number = cell.Value.GetNumber();
                    return Math.Round(number, decimalPlaces).ToString($"F{decimalPlaces}");
                }
                else if (cell.Value.IsText)
                {
                    var text = cell.Value.GetText();
                    // テキストが数値に変換できる場合は丸める
                    if (double.TryParse(text, out var number))
                    {
                        return Math.Round(number, decimalPlaces).ToString($"F{decimalPlaces}");
                    }
                    return text;
                }
                else if (cell.Value.IsDateTime)
                {
                    return cell.Value.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (cell.Value.IsBoolean)
                {
                    return cell.Value.GetBoolean().ToString();
                }
                else if (cell.Value.IsError)
                {
                    return $"#ERROR: {cell.Value.GetError()}";
                }
                else
                {
                    return cell.Value.GetText();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "セル値取得エラー: Address={CellAddress}", cellAddress);
            throw new InvalidOperationException($"セル '{cellAddress}' の値取得に失敗しました: {ex.Message}", ex);
        }
    }

    private async Task DownloadCsv()
    {
        if (comparisonResults == null || comparisonResults.Count == 0)
        {
            return;
        }

        try
        {
            var tempFile = Path.GetTempFileName().Replace(".tmp", ".csv");
            tempFiles.Add(tempFile);

            using (var writer = new StreamWriter(tempFile, false, System.Text.Encoding.UTF8))
            {
                await writer.WriteLineAsync("\uFEFF");
                await writer.WriteLineAsync("項目名,ブック1,セル1,数式1,値1,ブック2,セル2,数式2,値2,結果,メッセージ");

                foreach (var result in comparisonResults)
                {
                    var line = string.Join(",",
           QuoteCsv(result.ItemName),
          QuoteCsv(result.Book1Name),
            QuoteCsv(result.Sheet1Cell),
            QuoteCsv(result.Sheet1Formula),
            QuoteCsv(result.Sheet1Value?.ToString()),
                  QuoteCsv(result.Book2Name),
                 QuoteCsv(result.Sheet2Cell),
          QuoteCsv(result.Sheet2Formula),
             QuoteCsv(result.Sheet2Value?.ToString()),
                   QuoteCsv(result.IsMatch ? "一致" : "不一致"),
               QuoteCsv(result.Message)
           );

                    await writer.WriteLineAsync(line);
                }
            }

            using (var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
            {
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                await JS.InvokeVoidAsync("downloadFile", "comparison_results.csv", Convert.ToBase64String(memoryStream.ToArray()));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "CSV ダウンロードエラー");
        }
        finally
        {
            foreach (var file in tempFiles)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "一時ファイル削除エラー: " + file);
                }
            }
        }
    }
}
