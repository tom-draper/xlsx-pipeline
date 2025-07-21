using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Cells;

public class SetCellValueAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string CellAddress { get; set; }
    public required string Value { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = string.IsNullOrEmpty(SheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(SheetName);

            worksheet.Cell(CellAddress).Value = Value;
            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}