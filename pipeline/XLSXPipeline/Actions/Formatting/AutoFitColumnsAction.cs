using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Formatting;

public class AutoFitColumnsAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to auto-fit. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// First column to auto-fit, e.g. "A" or "3". Null means start from the first used column.
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Last column to auto-fit, e.g. "Z" or "10". Null means go to the last used column.
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// Minimum column width after fitting. Null means no minimum.
    /// </summary>
    public double? MinWidth { get; set; }

    /// <summary>
    /// Maximum column width after fitting. Null means no maximum.
    /// </summary>
    public double? MaxWidth { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        var columns = GetColumns(worksheet);

        if (MinWidth.HasValue && MaxWidth.HasValue)
            columns.AdjustToContents(MinWidth.Value, MaxWidth.Value);
        else
            columns.AdjustToContents();

        if (MinWidth.HasValue)
            foreach (var col in columns)
                if (col.Width < MinWidth.Value) col.Width = MinWidth.Value;

        if (MaxWidth.HasValue)
            foreach (var col in columns)
                if (col.Width > MaxWidth.Value) col.Width = MaxWidth.Value;

        workbook.Save();
        return Task.CompletedTask;
    }

    private IXLColumns GetColumns(IXLWorksheet worksheet)
    {
        if (!string.IsNullOrWhiteSpace(From) && !string.IsNullOrWhiteSpace(To))
            return worksheet.Columns(From, To);

        var used = worksheet.RangeUsed();
        if (used == null)
            return worksheet.Columns();

        int firstCol = used.FirstColumn().ColumnNumber();
        int lastCol = used.LastColumn().ColumnNumber();

        int fromCol = !string.IsNullOrWhiteSpace(From) ? ParseColumn(worksheet, From) : firstCol;
        int toCol = !string.IsNullOrWhiteSpace(To) ? ParseColumn(worksheet, To) : lastCol;

        return worksheet.Columns(fromCol, toCol);
    }

    private static int ParseColumn(IXLWorksheet worksheet, string column)
    {
        if (int.TryParse(column, out int colNum))
            return colNum;
        return worksheet.Column(column).ColumnNumber();
    }
}
