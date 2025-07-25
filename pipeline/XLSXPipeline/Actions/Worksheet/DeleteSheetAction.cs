using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class DeleteSheetAction : ActionBase
{
    public required string SheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            Validation.ValidateSheetExists(workbook, SheetName);
            var worksheet = Helpers.GetWorksheet(workbook, SheetName);

            worksheet.Delete();
            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
