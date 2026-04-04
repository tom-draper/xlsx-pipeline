using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Cells;

public class CopyRangeAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("sourceRange")]
    public required PlaceholderString SourceRange { get; set; }

    [JsonPropertyName("destinationCell")]
    public required PlaceholderString DestinationCell { get; set; }

    [JsonPropertyName("destinationSheet")]
    public PlaceholderString? DestinationSheet { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var sourceSheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        IXLWorksheet destSheet;
        if (string.IsNullOrEmpty(DestinationSheet))
            destSheet = sourceSheet;
        else
            destSheet = Helpers.GetWorksheetOrFirst(workbook, DestinationSheet);

        var sourceRange = sourceSheet.Range((string)SourceRange!);
        sourceRange.CopyTo(destSheet.Cell((string)DestinationCell!));

        workbook.Save();
        return Task.CompletedTask;
    }
}
