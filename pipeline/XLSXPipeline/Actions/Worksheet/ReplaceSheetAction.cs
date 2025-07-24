using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class ReplaceSheetAction : ActionBase
{
    public required string TargetSheetName { get; set; }
    public string? SourceFilePath { get; set; }
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

        var sourceWorksheet = GetWorksheet(workbook, SourceSheetName, "Source");
        var targetWorksheet = GetWorksheet(workbook, TargetSheetName, "Target");

        int targetPosition = targetWorksheet.Position;
        targetWorksheet.Delete();

        var copiedWorksheet = CopyAndRenameWorksheet(sourceWorksheet, workbook, TargetSheetName, targetPosition);

        workbook.Save();
    }

    private void ReplaceSheetFromDifferentWorkbook(string filePath)
    {
        using var targetWorkbook = new XLWorkbook(filePath);
        using var sourceWorkbook = new XLWorkbook(SourceFilePath);

        var sourceWorksheet = GetWorksheet(sourceWorkbook, SourceSheetName, "Source", SourceFilePath);
        var targetWorksheet = GetWorksheet(targetWorkbook, TargetSheetName, "Target");

        int targetPosition = targetWorksheet.Position;
        targetWorksheet.Delete();

        var copiedWorksheet = CopyAndRenameWorksheet(sourceWorksheet, targetWorkbook, TargetSheetName, targetPosition);

        targetWorkbook.Save();
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName, string sheetType, string? filePath = null)
    {
        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
        {
            string location = filePath != null ? $" in '{filePath}'" : "";
            throw new InvalidOperationException($"{sheetType} sheet '{sheetName}' does not exist{location}.");
        }
        return worksheet;
    }

    private static IXLWorksheet CopyAndRenameWorksheet(IXLWorksheet sourceWorksheet, XLWorkbook targetWorkbook, string newName, int position)
    {
        var copiedWorksheet = sourceWorksheet.CopyTo(targetWorkbook);
        copiedWorksheet.Name = newName;
        copiedWorksheet.Position = position;
        return copiedWorksheet;
    }
}