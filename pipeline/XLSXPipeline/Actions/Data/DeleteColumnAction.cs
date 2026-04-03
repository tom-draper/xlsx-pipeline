using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class DeleteColumnAction : ActionBase
{
    // Backing field for SheetName
    private string? _sheetName;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? SheetName
    {
        get => _sheetName != null ? Helpers.ReplaceDateTimePlaceholders(_sheetName) : null;
        set => _sheetName = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("sheetName")]
    public string? JsonSheetName
    {
        get => _sheetName;
        set => _sheetName = value;
    }
    public required string ColumnName { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        DeleteColumns(worksheet);

        workbook.Save();

        return Task.CompletedTask;
    }

    private void DeleteColumns(IXLWorksheet worksheet)
    {
        ValidateInputs();
        var startColumnNumber = GetColumnNumber(worksheet);
        ValidateColumnRange(worksheet, startColumnNumber);
        PerformColumnDeletion(worksheet, startColumnNumber);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(ColumnName))
            throw new ArgumentException("Column name cannot be null or empty.", nameof(ColumnName));

        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be greater than 0.");
    }

    private int GetColumnNumber(IXLWorksheet worksheet)
    {
        try
        {
            var column = worksheet.Column(ColumnName);
            return column.ColumnNumber();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Column '{ColumnName}' does not exist or is invalid.", ex);
        }
    }

    private void ValidateColumnRange(IXLWorksheet worksheet, int startColumnNumber)
    {
        int maxColumn = XLHelper.MaxColumnNumber;

        if (startColumnNumber + Count - 1 > maxColumn)
            throw new ArgumentOutOfRangeException(nameof(Count),
                $"Cannot delete {Count} columns starting from column {startColumnNumber}. " +
                $"This would exceed the maximum column limit of {maxColumn}.");

        // Check if we're trying to delete more columns than exist
        var lastUsedColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        if (lastUsedColumn > 0 && startColumnNumber > lastUsedColumn)
            throw new InvalidOperationException(
                $"Cannot delete column {startColumnNumber}. The worksheet only has data up to column {lastUsedColumn}.");
    }

    private void PerformColumnDeletion(IXLWorksheet worksheet, int startColumnNumber)
    {
        int endColumnNumber = startColumnNumber + Count - 1;
        worksheet.Columns(startColumnNumber, endColumnNumber).Delete();
    }
}