using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class HideSheetAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to hide. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// When true the sheet is very hidden — it cannot be unhidden via the Excel UI and
    /// requires code or this pipeline to restore it. Defaults to false (standard hidden).
    /// </summary>
    public bool VeryHidden { get; set; } = false;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        if (workbook.Worksheets.Count(ws => ws.Visibility == XLWorksheetVisibility.Visible) <= 1
            && worksheet.Visibility == XLWorksheetVisibility.Visible)
            throw new InvalidOperationException(
                $"Cannot hide sheet '{worksheet.Name}': at least one sheet must remain visible.");

        worksheet.Visibility = VeryHidden
            ? XLWorksheetVisibility.VeryHidden
            : XLWorksheetVisibility.Hidden;

        workbook.Save();
        return Task.CompletedTask;
    }
}
