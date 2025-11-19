using ClosedXML.Excel;
using Microsoft.AspNetCore.Components.Forms;
using ExcelDataReader;

namespace MesWEB.Components.Pages;

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
         errorMessage = "����5�t�@�C�����A�b�v���[�h����Ă��܂��B�t�@�C�����폜���Ă���ǉ����Ă��������B";
      return;
     }

          var files = e.GetMultipleFiles(remainingSlots);
            var newFilesCount = 0;

        foreach (var file in files)
      {
     if (file.Size > 10 * 1024 * 1024)
         {
        errorMessage = $"�t�@�C���u{file.Name}�v���傫�����܂��i�ő�10MB�j";
      continue;
     }

             if (uploadedBooks.Any(b => b.FileName == file.Name))
      {
          errorMessage = $"�t�@�C���u{file.Name}�v�͊��ɃA�b�v���[�h����Ă��܂�";
        continue;
     }

     templateMessage = $"�t�@�C���u{file.Name}�v��ǂݍ��ݒ�...";
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
         Logger.LogError(xlsEx, ".xls �t�@�C���̕ϊ��Ɏ��s: {File}", file.Name);
      throw new InvalidOperationException($".xls �t�@�C���̏����Ɏ��s���܂����B", xlsEx);
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
           Logger.LogInformation($"�t�@�C���ǂݍ��ݐ���: {file.Name}");

          templateMessage = $"{newFilesCount}�̃t�@�C����ǉ����܂����i���v{uploadedBooks.Count}�j";
   await InvokeAsync(StateHasChanged);
         }
       catch (Exception ex)
      {
             Logger.LogError(ex, "�t�@�C���ǂݍ��ݎ��s: {File}", file.Name);
                    errorMessage = $"�t�@�C���u{file.Name}�v�̓Ǎ��Ɏ��s���܂����B";
          continue;
       }
  }

   if (newFilesCount > 0)
  {
        templateMessage = $"{newFilesCount}�̃t�@�C����ǉ����܂����i���v{uploadedBooks.Count}�j";
      UpdateAllBookIndices();

 if (cellMappings.Count == 0)
       {
            AddMapping();
    }
}
        }
        catch (Exception ex)
        {
  errorMessage = $"�t�@�C���A�b�v���[�h�����ŃG���[���������܂���: {ex.Message}";
          Logger.LogError(ex, "HandleFileUpload error");
        }
    finally
        {
            isLoading = false;
     await InvokeAsync(StateHasChanged);
        }
    }
}
