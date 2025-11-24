using Microsoft.AspNetCore.Components.Forms;
using ClosedXML.Excel;
using ExcelDataReader;
using MesWEB.Shared.Models;
using Microsoft.Extensions.Logging;

namespace MesWEB.ExcelCompare.Components.Pages;

public partial class ExcelCompare
{
    private async Task HandleFileUpload(InputFileChangeEventArgs e)
    {
        isLoading = true;
        errorMessage = string.Empty;
        comparisonResults = null;

        await InvokeAsync(StateHasChanged);

        try
        {
            var remainingSlots = 5 - uploadedBooks.Count;
            if (remainingSlots <= 0)
            {
                errorMessage = "既に5ファイルがアップロードされています。ファイルを削除してから追加してください。";
                return;
            }

            var files = e.GetMultipleFiles(remainingSlots);
            var newFilesCount = 0;

            foreach (var file in files)
            {
                if (file.Size > 10 * 1024 * 1024)
                {
                    errorMessage = $"ファイル「{file.Name}」が大きすぎます（最大10MB）";
                    continue;
                }

                if (uploadedBooks.Any(b => b.FileName == file.Name))
                {
                    errorMessage = $"ファイル「{file.Name}」は既にアップロードされています";
                    continue;
                }

                templateMessage = $"ファイル「{file.Name}」を読み込み中...";
                await InvokeAsync(StateHasChanged);

                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                var nameLower = (file.Name ?? "").ToLowerInvariant();

                try
                {
                    XLWorkbook workbook;

                    if (nameLower.EndsWith(".xls"))
                    {
                        try
                        {
                            memoryStream.Position = 0;
                            using var reader = ExcelReaderFactory.CreateReader(memoryStream);
                            var conf = new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = _ => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                            };
                            var ds = reader.AsDataSet(conf);

                            workbook = new XLWorkbook();
                            foreach (System.Data.DataTable table in ds.Tables)
                            {
                                var sheetName = string.IsNullOrWhiteSpace(table.TableName) ? "Sheet" : table.TableName;
                                var uniqueName = sheetName;
                                var idx = 1;
                                while (workbook.Worksheets.Any(ws => ws.Name == uniqueName))
                                {
                                    uniqueName = $"{sheetName}_{idx++}";
                                }
                                var ws = workbook.Worksheets.Add(uniqueName);

                                for (int r = 0; r < table.Rows.Count; r++)
                                {
                                    for (int c = 0; c < table.Columns.Count; c++)
                                    {
                                        var cellValue = table.Rows[r][c];
                                        if (cellValue != null && cellValue != DBNull.Value)
                                        {
                                            ws.Cell(r + 1, c + 1).Value = cellValue.ToString() ?? "";
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception xlsEx)
                        {
                            Logger.LogError(xlsEx, ".xls ファイルの変換に失敗: {File}", file.Name);
                            throw new InvalidOperationException($".xls ファイルの処理に失敗しました。", xlsEx);
                        }
                    }
                    else
                    {
                        memoryStream.Position = 0;
                        workbook = new XLWorkbook(memoryStream);
                    }

                    uploadedBooks.Add(new UploadedBook
                    {
                        FileName = file.Name,
                        Workbook = workbook
                    });

                    newFilesCount++;
                    Logger.LogInformation($"ファイル読み込み成功: {file.Name}");

                    templateMessage = $"{newFilesCount}個のファイルを追加しました（合計{uploadedBooks.Count}個）";
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "ファイル読み込み失敗: {File}", file.Name);
                    errorMessage = $"ファイル「{file.Name}」の読込に失敗しました。";
                    continue;
                }
            }

            if (newFilesCount > 0)
            {
                templateMessage = $"{newFilesCount}個のファイルを追加しました（合計{uploadedBooks.Count}個）";
                UpdateAllBookIndices();

                if (cellMappings.Count == 0)
                {
                    AddMapping();
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"ファイルアップロード処理でエラーが発生しました: {ex.Message}";
            Logger.LogError(ex, "HandleFileUpload error");
        }
        finally
        {
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
