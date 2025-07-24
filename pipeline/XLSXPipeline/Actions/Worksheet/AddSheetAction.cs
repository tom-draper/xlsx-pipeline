using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class AddSheetAction : ActionBase
{
    public required string SheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            ValidateNewSheetName(workbook, SheetName);

            workbook.AddWorksheet(SheetName);
            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private static void ValidateNewSheetName(XLWorkbook workbook, string newSheetName)
    {
        if (workbook.Worksheets.Any(ws => ws.Name == newSheetName))
            throw new InvalidOperationException($"Sheet '{newSheetName}' already exists in the target workbook.");
    }
}
