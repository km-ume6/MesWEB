using MesWEB.Models;
using MesWEB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace MesWEB.Components.Pages;

public partial class ExcelCompare
{
    private async Task LoadSavedTemplates()
    {
        try
        {
            // �^�C���A�E�g�t���ŃN�G�����s�i10�b�ɉ����j
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            Logger.LogInformation("�f�[�^�x�[�X����e���v���[�g��ǂݍ���ł��܂�...");

            // DbContextFactory����V�����C���X�^���X���擾
            await using var db = await DbFactory.CreateDbContextAsync(cts.Token);

            savedTemplates = await db.CellMappingTemplates
             .Include(t => t.MappingItems)
         .OrderBy(t => t.TemplateName)
    .AsNoTracking() // �p�t�H�[�}���X����
       .ToListAsync(cts.Token);

            Logger.LogInformation($"�e���v���[�g�ǂݍ��݊���: {savedTemplates.Count}��");

     // �f�o�b�O: �e���v���[�g�������O�o��
            foreach (var template in savedTemplates)
            {
         Logger.LogInformation($"  - {template.TemplateName} (ID: {template.TemplateId}, Items: {template.MappingItems?.Count ?? 0})");
  }
      }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, "�e���v���[�g�ǂݍ��݂��^�C���A�E�g���܂����i10�b�j");
            if (savedTemplates == null)
{
                savedTemplates = new List<CellMappingTemplate>();
            }
       templateMessage = "�e���v���[�g�̓ǂݍ��݂��^�C���A�E�g���܂����B�f�[�^�x�[�X�ڑ����m�F���Ă��������B";
      throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "�e���v���[�g�ǂݍ��݃G���[: {Message}", ex.Message);
        Logger.LogError(ex, "InnerException: {InnerMessage}", ex.InnerException?.Message);

            if (savedTemplates == null)
        {
            savedTemplates = new List<CellMappingTemplate>();
            }
            templateMessage = $"�e���v���[�g�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}";
            throw;
        }
    }

    private async Task SaveTemplate()
    {
     try
        {
     // �㏑������: �����e���v���[�g���I������Ă���ꍇ�͏㏑���D��
          if (selectedTemplateId > 0)
       {
                await UpdateTemplate();
          return;
 }

   if (string.IsNullOrWhiteSpace(newTemplateName))
  {
       templateMessage = "�e���v���[�g������͂��Ă�������";
     return;
            }

  var template = new CellMappingTemplate
          {
    TemplateName = newTemplateName.Trim(),
    Description = newTemplateDescription?.Trim(),
       CreatedAt = DateTime.UtcNow,
       UpdatedAt = DateTime.UtcNow
            };

            var sortOrder = 0;
    foreach (var mapping in cellMappings)
   {
    // �t�@�C�����ƃV�[�g���ŕۑ�
      var sheet1Name = $"{mapping.Book1FileName}:{mapping.Sheet1Name}";
 var sheet2Name = $"{mapping.Book2FileName}:{mapping.Sheet2Name}";

       Logger.LogInformation($"�ۑ�: {mapping.ItemName} -> Sheet1={sheet1Name}, Sheet2={sheet2Name}");

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

 Logger.LogInformation($"�e���v���[�g�u{newTemplateName}�v��V�K�ۑ����܂����iID: {template.TemplateId}, Items: {template.MappingItems.Count}�j");
            templateMessage = $"�e���v���[�g�u{newTemplateName}�v��ۑ����܂����i{template.MappingItems.Count}���ځj";
    newTemplateName = string.Empty;
      newTemplateDescription = string.Empty;

            await LoadSavedTemplates();

      await InvokeAsync(StateHasChanged);
      }
        catch (Exception ex)
        {
            templateMessage = $"�ۑ��G���[: {ex.Message}";
            Logger.LogError(ex, "�e���v���[�g�ۑ��G���[: {Message}, InnerException: {Inner}", ex.Message, ex.InnerException?.Message);
      await InvokeAsync(StateHasChanged);
}
    }

    private async Task UpdateTemplate()
    {
     try
     {
    if (selectedTemplateId == 0)
{
       templateMessage = "�e���v���[�g��I�����Ă�������";
       return;
}

     // ���O�������͂̏ꍇ�͊��������g�p
     var templateName = string.IsNullOrWhiteSpace(newTemplateName)
       ? savedTemplates.FirstOrDefault(t => t.TemplateId == selectedTemplateId)?.TemplateName
           : newTemplateName.Trim();

            if (string.IsNullOrWhiteSpace(templateName))
            {
          templateMessage = "�e���v���[�g������͂��Ă�������";
       return;
     }

         await using var db = await DbFactory.CreateDbContextAsync();
  
  // �����e���v���[�g���擾�i�g���b�L���O�L���j
        var template = await db.CellMappingTemplates
  .Include(t => t.MappingItems)
      .FirstOrDefaultAsync(t => t.TemplateId == selectedTemplateId);

            if (template == null)
   {
  templateMessage = "�I�������e���v���[�g��������܂���";
          Logger.LogWarning($"�e���v���[�gID {selectedTemplateId} ��������܂���ł���");
   return;
            }

            Logger.LogInformation($"�e���v���[�g�u{template.TemplateName}�v�iID: {selectedTemplateId}�j���㏑���X�V���܂��B�����A�C�e����: {template.MappingItems.Count}");

    template.TemplateName = templateName;
            template.Description = newTemplateDescription?.Trim();
   template.UpdatedAt = DateTime.UtcNow;

            // �����̃A�C�e�������S�ɍ폜�iRemoveRange�Ŋm���ɍ폜�j
    if (template.MappingItems.Any())
            {
    db.CellMappingItems.RemoveRange(template.MappingItems);
           Logger.LogInformation($"{template.MappingItems.Count} ���̊����A�C�e�����폜�L���[�ɒǉ����܂���");
            }

            // �V�����}�b�s���O��ǉ�
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
      Logger.LogInformation($"�V�K�A�C�e����ǉ�: {mapping.ItemName} -> Sheet1={sheet1Name}, Sheet2={sheet2Name}");
            }

        // �ύX��ۑ�
            var changes = await db.SaveChangesAsync();
 Logger.LogInformation($"�e���v���[�g�X�V����: {changes} ���̕ύX��ۑ����܂���");

            templateMessage = $"�e���v���[�g�u{templateName}�v���㏑���X�V���܂����i{cellMappings.Count}���ځj";
   newTemplateName = string.Empty;
        newTemplateDescription = string.Empty;

            await LoadSavedTemplates();
   await InvokeAsync(StateHasChanged);
        }
        catch (DbUpdateException dbEx)
        {
  templateMessage = $"�f�[�^�x�[�X�X�V�G���[: {dbEx.InnerException?.Message ?? dbEx.Message}";
            Logger.LogError(dbEx, "�e���v���[�g�X�V����DB��O: {Message}, InnerException: {Inner}", dbEx.Message, dbEx.InnerException?.Message);
await InvokeAsync(StateHasChanged);
        }
     catch (Exception ex)
   {
       templateMessage = $"�X�V�G���[: {ex.Message}";
     Logger.LogError(ex, "�e���v���[�g�X�V�G���[: {Message}, InnerException: {Inner}", ex.Message, ex.InnerException?.Message);
 await InvokeAsync(StateHasChanged);
        }
    }

  private async Task LoadTemplate()
    {
        try
        {
 if (selectedTemplateId == 0)
   {
      templateMessage = "�e���v���[�g��I�����Ă�������";
                return;
     }

     await using var db = await DbFactory.CreateDbContextAsync();
          var template = await db.CellMappingTemplates
          .Include(t => t.MappingItems)
     .FirstOrDefaultAsync(t => t.TemplateId == selectedTemplateId);

     if (template == null)
 {
             templateMessage = "�I�������e���v���[�g��������܂���";
   return;
            }

            cellMappings.Clear();

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
   }

            UpdateAllBookIndices();

            newTemplateName = template.TemplateName;
            newTemplateDescription = template.Description ?? "";
            templateMessage = $"�e���v���[�g�u{template.TemplateName}�v��ǂݍ��݂܂����i{cellMappings.Count}���ځj";

      StateHasChanged();
    }
        catch (Exception ex)
   {
            templateMessage = $"�ǂݍ��݃G���[: {ex.Message}";
            Logger.LogError(ex, "�e���v���[�g�ǂݍ��݃G���[");
   }
    }

private async Task DeleteTemplate()
    {
     try
      {
            if (selectedTemplateId == 0)
            {
    templateMessage = "�e���v���[�g��I�����Ă�������";
                return;
         }

  var templateName = savedTemplates.FirstOrDefault(t => t.TemplateId == selectedTemplateId)?.TemplateName ?? $"ID:{selectedTemplateId}";
 var confirmed = await JS.InvokeAsync<bool>("confirm", new object[] { $"�e���v���[�g�u{templateName}�v���폜���܂����H" });
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
     Logger.LogInformation($"�e���v���[�g�u{templateName}�v�iID: {selectedTemplateId}�j���폜���܂���");
            }

         templateMessage = $"�e���v���[�g�u{templateName}�v���폜���܂���";
            selectedTemplateId = 0;
          newTemplateName = string.Empty;
    newTemplateDescription = string.Empty;

            await LoadSavedTemplates();
  await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            templateMessage = $"�폜�G���[: {ex.Message}";
   Logger.LogError(ex, "�e���v���[�g�폜�G���[");
       await InvokeAsync(StateHasChanged);
        }
    }
}
