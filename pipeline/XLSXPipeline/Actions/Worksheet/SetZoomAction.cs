using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class SetZoomAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("zoomScale")]
    public required int ZoomScale { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        if (ZoomScale < 10 || ZoomScale > 400)
            throw new ArgumentOutOfRangeException(nameof(ZoomScale), "ZoomScale must be between 10 and 400.");

        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        worksheet.SheetView.ZoomScale = ZoomScale;

        workbook.Save();
        return Task.CompletedTask;
    }
}
