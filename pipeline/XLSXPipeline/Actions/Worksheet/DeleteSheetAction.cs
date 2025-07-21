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
            var worksheet = workbook.Worksheet(SheetName);

            if (worksheet == null)
                throw new InvalidOperationException($"Sheet '{SheetName}' does not exist.");

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
