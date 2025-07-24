using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class CopySheetAction : ActionBase
{
    public required string SourceSheetName { get; set; }
    public required string NewSheetName { get; set; }
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

        var sourceWorksheet = GetWorksheet(workbook, SourceSheetName);
        ValidateNewSheetName(workbook, NewSheetName);

        sourceWorksheet.CopyTo(NewSheetName);
        workbook.Save();
    }

    private void CopySheetToDifferentWorkbook(string filePath)
    {
        using var sourceWorkbook = new XLWorkbook(filePath);
        using var targetWorkbook = LoadOrCreateTargetWorkbook(TargetFilePath!);

        var sourceWorksheet = GetWorksheet(sourceWorkbook, SourceSheetName);
        ValidateNewSheetName(targetWorkbook, NewSheetName);

        sourceWorksheet.CopyTo(targetWorkbook, NewSheetName);
        targetWorkbook.SaveAs(TargetFilePath!);
    }

    private static XLWorkbook LoadOrCreateTargetWorkbook(string targetFilePath)
    {
        return System.IO.File.Exists(targetFilePath)
            ? new XLWorkbook(targetFilePath)
            : new XLWorkbook();
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName)
    {
        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Sheet '{sheetName}' does not exist.");
        return worksheet;
    }

    private static void ValidateNewSheetName(XLWorkbook workbook, string newSheetName)
    {
        if (workbook.Worksheets.Any(ws => ws.Name == newSheetName))
            throw new InvalidOperationException($"Sheet '{newSheetName}' already exists in the target workbook.");
    }
}