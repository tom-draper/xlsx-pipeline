using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class UnhideSheetAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to make visible. Required — hidden sheets cannot be auto-detected.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public required PlaceholderString SheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        // workbook.Worksheets includes hidden and very-hidden sheets
        string sheetName = SheetName;
        var worksheet = workbook.Worksheets
            .FirstOrDefault(ws => ws.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Sheet '{sheetName}' does not exist in the workbook.");

        worksheet.Visibility = XLWorksheetVisibility.Visible;

        workbook.Save();
        return Task.CompletedTask;
    }
}
