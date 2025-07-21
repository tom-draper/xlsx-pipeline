using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class MergeDataAction : ActionBase
{
    public required string SourceFilePath { get; set; }
    public string? SourceSheetName { get; set; }
    public string? DestinationSheetName { get; set; }
    public string DestinationCell { get; set; } = "A1";
    public bool IncludeHeaders { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var destWorkbook = new XLWorkbook(filePath);
            using var sourceWorkbook = new XLWorkbook(SourceFilePath);

            var sourceSheet = string.IsNullOrEmpty(SourceSheetName)
                ? sourceWorkbook.Worksheets.First()
                : sourceWorkbook.Worksheet(SourceSheetName);

            var destSheet = string.IsNullOrEmpty(DestinationSheetName)
                ? destWorkbook.Worksheets.First()
                : destWorkbook.Worksheet(DestinationSheetName);

            var usedRange = sourceSheet.RangeUsed();
            if (usedRange != null)
            {
                var startRow = IncludeHeaders ? 1 : 2;
                var dataRange = sourceSheet.Range(startRow, 1, usedRange.LastRow().RowNumber(), usedRange.LastColumn().ColumnNumber());
                dataRange.CopyTo(destSheet.Cell(DestinationCell));
            }

            destWorkbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
