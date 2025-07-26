using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class ReplaceSheetAction : ActionBase
{
    [ReplacePlaceholders]
    public required string TargetSheetName { get; set; }
    [ReplacePlaceholders]
    public string? SourceFilePath { get; set; }
    [ReplacePlaceholders]
    public required string SourceSheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (SourceFilePath == null)
                ReplaceSheetWithinSameWorkbook(filePath);
            else
                ReplaceSheetFromDifferentWorkbook(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void ReplaceSheetWithinSameWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        Validation.ValidateSheetExists(workbook, SourceSheetName);
        Validation.ValidateSheetExists(workbook, TargetSheetName);
        var sourceWorksheet = Helpers.GetWorksheet(workbook, SourceSheetName);
        var targetWorksheet = Helpers.GetWorksheet(workbook, TargetSheetName);

        int targetPosition = targetWorksheet.Position;
        targetWorksheet.Delete();

        var copiedWorksheet = CopyAndRenameWorksheet(sourceWorksheet, workbook, TargetSheetName, targetPosition);

        workbook.Save();
    }

    private void ReplaceSheetFromDifferentWorkbook(string filePath)
    {
        using var targetWorkbook = new XLWorkbook(filePath);
        using var sourceWorkbook = new XLWorkbook(SourceFilePath);

        Validation.ValidateSheetExists(sourceWorkbook, SourceSheetName);
        Validation.ValidateSheetExists(targetWorkbook, TargetSheetName);
        var sourceWorksheet = Helpers.GetWorksheet(sourceWorkbook, SourceSheetName);
        var targetWorksheet = Helpers.GetWorksheet(targetWorkbook, TargetSheetName);

        int targetPosition = targetWorksheet.Position;
        targetWorksheet.Delete();

        var copiedWorksheet = CopyAndRenameWorksheet(sourceWorksheet, targetWorkbook, TargetSheetName, targetPosition);

        targetWorkbook.Save();
    }

    private static IXLWorksheet CopyAndRenameWorksheet(IXLWorksheet sourceWorksheet, XLWorkbook targetWorkbook, string newName, int position)
    {
        var copiedWorksheet = sourceWorksheet.CopyTo(targetWorkbook);
        copiedWorksheet.Name = newName;
        copiedWorksheet.Position = position;
        return copiedWorksheet;
    }
}