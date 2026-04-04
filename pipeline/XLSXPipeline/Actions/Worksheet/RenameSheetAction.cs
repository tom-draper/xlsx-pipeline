using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class RenameSheetAction : ActionBase
{
    /// <summary>
    /// Sheet name to rename. Automatically processes date/time placeholders.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// New name for the sheet. Automatically processes date/time placeholders.
    /// </summary>
    [JsonPropertyName("newSheetName")]
    public PlaceholderString? NewSheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        Validation.ValidateSheetExists(workbook, SheetName);
        var worksheet = Helpers.GetWorksheet(workbook, SheetName);
        Validation.ValidateSheetNotExists(workbook, NewSheetName);

        worksheet.Name = NewSheetName!;
        workbook.Save();

        return Task.CompletedTask;
    }
}
