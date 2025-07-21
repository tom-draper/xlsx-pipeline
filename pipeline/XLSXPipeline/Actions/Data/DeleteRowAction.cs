using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class DeleteRowAction : ActionBase
{
    public string? SheetName { get; set; }
    public int RowNumber { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = string.IsNullOrEmpty(SheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(SheetName);

            worksheet.Rows(RowNumber, RowNumber + Count - 1).Delete();
            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}