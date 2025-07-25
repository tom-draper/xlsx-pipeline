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

            Validation.ValidateSheetNotExists(workbook, SheetName);

            workbook.AddWorksheet(SheetName);
            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
