using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Formatting;

public class SetRowHeightAction : ActionBase
{
    /// <summary>
    /// Name of the sheet. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// 1-based row number to start from.
    /// </summary>
    public required int Row { get; set; }

    /// <summary>
    /// Height in points.
    /// </summary>
    public required double Height { get; set; }

    /// <summary>
    /// Number of consecutive rows to set. Defaults to 1.
    /// </summary>
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        if (Row < 1)
            throw new ArgumentOutOfRangeException(nameof(Row), "Row must be at least 1.");
        if (Height < 0)
            throw new ArgumentOutOfRangeException(nameof(Height), "Height cannot be negative.");
        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be at least 1.");

        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        for (int i = 0; i < Count; i++)
            worksheet.Row(Row + i).Height = Height;

        workbook.Save();
        return Task.CompletedTask;
    }
}
