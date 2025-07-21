using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Advanced;

public class CreatePivotTableAction : ActionBase
{
    public string? SourceSheetName { get; set; }
    public required string SourceRange { get; set; }
    public required string DestinationSheetName { get; set; }
    public required string DestinationCell { get; set; }
    public List<string> RowFields { get; set; } = [];
    public List<string> ColumnFields { get; set; } = [];
    public List<string> DataFields { get; set; } = [];

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var sourceSheet = string.IsNullOrEmpty(SourceSheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(SourceSheetName);

            var destSheet = workbook.Worksheet(DestinationSheetName) ?? workbook.Worksheets.Add(DestinationSheetName);

            var sourceRange = sourceSheet.Range(SourceRange);
            var pivotTable = destSheet.PivotTables.Add("PivotTable1", destSheet.Cell(DestinationCell), sourceRange);

            foreach (var field in RowFields)
                pivotTable.RowLabels.Add(field);

            foreach (var field in ColumnFields)
                pivotTable.ColumnLabels.Add(field);

            foreach (var field in DataFields)
                pivotTable.Values.Add(field);

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
