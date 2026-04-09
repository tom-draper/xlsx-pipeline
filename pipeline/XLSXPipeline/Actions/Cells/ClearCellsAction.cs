using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Cells;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClearType
{
    /// <summary>Clear cell values and formulas only, preserve formatting.</summary>
    Values,
    /// <summary>Clear formatting only, preserve values.</summary>
    Formats,
    /// <summary>Clear everything — values, formulas, and formatting.</summary>
    All
}

public class ClearCellsAction : ActionBase
{
    /// <summary>
    /// Name of the sheet. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Cell range to clear, e.g. "A1:D10", "B:D", or "3:5".
    /// </summary>
    public required string Range { get; set; }

    /// <summary>
    /// What to clear: Values, Formats, or All (default).
    /// </summary>
    public ClearType ClearType { get; set; } = ClearType.All;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        var options = ClearType switch
        {
            ClearType.Values  => XLClearOptions.Contents,
            ClearType.Formats => XLClearOptions.NormalFormats,
            ClearType.All     => XLClearOptions.All,
            _ => XLClearOptions.All
        };

        worksheet.Range(Range).Clear(options);

        workbook.Save();
        return Task.CompletedTask;
    }
}
