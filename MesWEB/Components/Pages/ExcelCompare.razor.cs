using ClosedXML.Excel;
using MesWEB.Models;
using MesWEB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace MesWEB.Components.Pages;

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
    private List<string> tempFiles = new(); // CSV �_�E�����[�h�p�̈ꎞ�t�@�C�����X�g

    // �e���v���[�g�֘A
    private List<CellMappingTemplate> savedTemplates = new();
    private int selectedTemplateId = 0;
    private string newTemplateName = string.Empty;
    private string newTemplateDescription = string.Empty;
private string templateMessage = string.Empty;

    // �w���v���[�_��
  private bool showHelp = false;

  [Inject] private ILogger<ExcelCompare> Logger { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // �������b�Z�[�W��ݒ�
        templateMessage = "�e���v���[�g��ǂݍ���ł��܂�...";
    
        // �e���v���[�g�ǂݍ��݂�x�����s�iUI�u���b�N��h���j
   await Task.Yield();
      
        try
 {
        await LoadSavedTemplates();
            
      // �ǂݍ��ݐ������UI���X�V
         if (savedTemplates != null && savedTemplates.Count > 0)
     {
          templateMessage = $"{savedTemplates.Count}���̃e���v���[�g��ǂݍ��݂܂���";
            }
   else
      {
                templateMessage = "�ۑ����ꂽ�e���v���[�g�͂���܂���";
}
 }
     catch (Exception ex)
        {
            Logger.LogError(ex, "OnInitializedAsync: �e���v���[�g�ǂݍ��݃G���[");
      templateMessage = $"�G���[: {ex.Message}";
   
       // �G���[������̃��X�g���m�ۂ���UI��\��
     if (savedTemplates == null)
     {
          savedTemplates = new List<CellMappingTemplate>();
        }
        }
 finally
        {
            // UI�������I�ɍX�V
            StateHasChanged();
        }
    }

    // �t�@�C��������u�b�N�C���f�b�N�X���X�V�i�����I�ɂ̂ݎg�p�j
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

    // �S�}�b�s���O�̃u�b�N�C���f�b�N�X���X�V
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
       ItemName = $"����{cellMappings.Count + 1}",
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

         templateMessage = $"�t�@�C���u{fileName}�v���폜���܂����i�c��{uploadedBooks.Count}�j";

            // �S�}�b�s���O�̃C���f�b�N�X���Čv�Z
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

   // �C���f�b�N�X�����Z�b�g�i�t�@�C�����͕ێ��j
      foreach (var mapping in cellMappings)
        {
            mapping.Book1Index = -1;
  mapping.Book2Index = -1;
     }

        templateMessage = "�S�Ẵt�@�C�����N���A���܂���";
    }

    private string QuoteCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
      return "";
        }

        // �J���}�A�_�u���N�H�[�e�[�V�������G�X�P�[�v
        value = value.Replace("\"", "\"\"");

        // �_�u���N�H�[�e�[�V�����ň͂�
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
