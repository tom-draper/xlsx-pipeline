using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Formatting;

public class SetColumnWidthAction : ActionBase
{
    /// <summary>
    /// Name of the sheet. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Starting column, as a letter ("A") or 1-based number ("3").
    /// </summary>
    public required string Column { get; set; }

    /// <summary>
    /// Width in Excel character units.
    /// </summary>
    public required double Width { get; set; }

    /// <summary>
    /// Number of consecutive columns to set. Defaults to 1.
    /// </summary>
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        if (Width < 0)
            throw new ArgumentOutOfRangeException(nameof(Width), "Width cannot be negative.");
        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be at least 1.");

        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        int startCol = ParseColumn(worksheet, Column);

        for (int i = 0; i < Count; i++)
            worksheet.Column(startCol + i).Width = Width;

        workbook.Save();
        return Task.CompletedTask;
    }

    private static int ParseColumn(IXLWorksheet worksheet, string column)
    {
        if (int.TryParse(column, out int colNum))
            return colNum;
        return worksheet.Column(column).ColumnNumber();
    }
}
