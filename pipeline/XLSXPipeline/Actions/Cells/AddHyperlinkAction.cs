using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Cells;

public class AddHyperlinkAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("cell")]
    public required PlaceholderString Cell { get; set; }

    [JsonPropertyName("url")]
    public required PlaceholderString Url { get; set; }

    [JsonPropertyName("tooltip")]
    public PlaceholderString? Tooltip { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        var hyperlink = new XLHyperlink((string)Url!);
        if (!string.IsNullOrEmpty(Tooltip))
            hyperlink.Tooltip = Tooltip;

        worksheet.Cell((string)Cell!).SetHyperlink(hyperlink);

        workbook.Save();
        return Task.CompletedTask;
    }
}
