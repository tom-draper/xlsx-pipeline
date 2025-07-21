using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class SortDataAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string Range { get; set; }
    public required string SortColumn { get; set; }
    public bool Ascending { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = string.IsNullOrEmpty(SheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(SheetName);

            var range = worksheet.Range(Range);
            var sortRange = range.Sort(SortColumn, Ascending ? XLSortOrder.Ascending : XLSortOrder.Descending);

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}