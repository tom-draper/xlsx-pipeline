using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class CopySheetAction : ActionBase
{
    [ReplacePlaceholders]
    public required string SourceSheetName { get; set; }
    [ReplacePlaceholders]
    public required string NewSheetName { get; set; }
    [ReplacePlaceholders]
    public string? TargetFilePath { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (TargetFilePath == null || TargetFilePath == filePath)
                CopySheetWithinSameWorkbook(filePath);
            else
                CopySheetToDifferentWorkbook(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void CopySheetWithinSameWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        Validation.ValidateSheetExists(workbook, NewSheetName);
        var worksheet = Helpers.GetWorksheet(workbook, SourceSheetName);
        Validation.ValidateSheetNotExists(workbook, NewSheetName);

        worksheet.CopyTo(NewSheetName);
        workbook.Save();
    }

    private void CopySheetToDifferentWorkbook(string filePath)
    {
        using var sourceWorkbook = new XLWorkbook(filePath);
        using var targetWorkbook = Helpers.GetOrCreateWorkbook(TargetFilePath!);

        Validation.ValidateSheetExists(sourceWorkbook, NewSheetName);
        var sourceWorksheet = Helpers.GetWorksheet(sourceWorkbook, SourceSheetName);
        Validation.ValidateSheetNotExists(targetWorkbook, NewSheetName);

        sourceWorksheet.CopyTo(targetWorkbook, NewSheetName);
        targetWorkbook.SaveAs(TargetFilePath!);
    }
}