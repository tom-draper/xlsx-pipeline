using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class SetPageOrientationAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("orientation")]
    public required string Orientation { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        worksheet.PageSetup.PageOrientation = Orientation.Trim().ToLowerInvariant() switch
        {
            "landscape" => XLPageOrientation.Landscape,
            "portrait" => XLPageOrientation.Portrait,
            _ => throw new ArgumentException($"Invalid orientation '{Orientation}'. Must be 'Landscape' or 'Portrait'.")
        };

        workbook.Save();
        return Task.CompletedTask;
    }
}
