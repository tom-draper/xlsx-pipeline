using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class DeleteColumnAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string ColumnName { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = string.IsNullOrEmpty(SheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(SheetName);

            var column = worksheet.Column(ColumnName);
            var startColumnNumber = column.ColumnNumber();

            // Delete the specified number of columns starting from the given column
            worksheet.Columns(startColumnNumber, startColumnNumber + Count - 1).Delete();

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}