using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class SetSheetTabColorAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to recolor. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Tab color as a hex string (e.g. "#FF0000") or a named XLColor (e.g. "Red").
    /// Set to null or empty to remove the tab color.
    /// </summary>
    public string? Color { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        worksheet.TabColor = ParseColor(Color);

        workbook.Save();
        return Task.CompletedTask;
    }

    private static XLColor ParseColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return XLColor.NoColor;

        try
        {
            return color.StartsWith('#')
                ? XLColor.FromHtml(color)
                : XLColor.FromName(color);
        }
        catch
        {
            throw new ArgumentException(
                $"Invalid color value '{color}'. Use a hex string (e.g. '#FF0000') or a named color (e.g. 'Red').");
        }
    }
}
