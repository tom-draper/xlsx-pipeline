using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class RenameSheetAction : ActionBase
{
    public required string SheetName { get; set; }
    public required string NewSheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = GetWorksheet(workbook, SheetName);
            ValidateNewSheetName(workbook, NewSheetName);

            worksheet.Name = NewSheetName;
            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName)
    {
        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Source sheet '{sheetName}' does not exist.");
        return worksheet;
    }

    private static void ValidateNewSheetName(XLWorkbook workbook, string newSheetName)
    {
        if (workbook.Worksheets.Any(ws => ws.Name == newSheetName))
            throw new InvalidOperationException($"Sheet '{newSheetName}' already exists in the target workbook.");
    }
}