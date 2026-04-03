using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class SortDataAction : ActionBase
{
    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    public required string Range { get; set; }
    public required string SortColumn { get; set; }
    public bool Ascending { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        SortRange(worksheet);

        workbook.Save();

        return Task.CompletedTask;
    }

    private void SortRange(IXLWorksheet worksheet)
    {
        ValidateInputs();

        var range = GetTargetRange(worksheet);
        ValidateRangeAndColumn(range);

        PerformSort(range);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(Range))
            throw new ArgumentException("Range cannot be null or empty.", nameof(Range));

        if (string.IsNullOrWhiteSpace(SortColumn))
            throw new ArgumentException("Sort column cannot be null or empty.", nameof(SortColumn));
    }

    private IXLRange GetTargetRange(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Range(Range);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid range specification '{Range}'. Please ensure the range is valid (e.g., 'A1:C10').", ex);
        }
    }

    private void ValidateRangeAndColumn(IXLRange range)
    {
        if (range.IsEmpty())
            throw new InvalidOperationException($"The specified range '{Range}' is empty or contains no data.");

        // Validate that the sort column exists within the range
        try
        {
            var sortColumnIndex = GetColumnIndex(SortColumn);
            var rangeFirstColumn = range.FirstColumn().ColumnNumber();
            var rangeLastColumn = range.LastColumn().ColumnNumber();

            if (sortColumnIndex < rangeFirstColumn || sortColumnIndex > rangeLastColumn)
                throw new ArgumentOutOfRangeException(nameof(SortColumn),
                    $"Sort column '{SortColumn}' is not within the specified range '{Range}'. " +
                    $"Range spans columns {XLHelper.GetColumnLetterFromNumber(rangeFirstColumn)} to {XLHelper.GetColumnLetterFromNumber(rangeLastColumn)}.");
        }
        catch (Exception ex) when (!(ex is ArgumentOutOfRangeException))
        {
            throw new ArgumentException($"Invalid sort column specification '{SortColumn}'. Please use a valid column reference (e.g., 'A', 'B', 'AA').", nameof(SortColumn), ex);
        }
    }

    private static int GetColumnIndex(string columnReference)
    {
        try
        {
            // Handle both letter-based (A, B, AA) and number-based column references
            if (int.TryParse(columnReference, out int columnNumber))
            {
                if (columnNumber < 1 || columnNumber > XLHelper.MaxColumnNumber)
                    throw new ArgumentOutOfRangeException(nameof(columnReference),
                        $"Column number {columnNumber} is out of valid range (1 to {XLHelper.MaxColumnNumber}).");
                return columnNumber;
            }

            return XLHelper.GetColumnNumberFromLetter(columnReference);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid column reference '{columnReference}'.", ex);
        }
    }

    private void PerformSort(IXLRange range)
    {
        try
        {
            var sortOrder = Ascending ? XLSortOrder.Ascending : XLSortOrder.Descending;
            range.Sort(SortColumn, sortOrder);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to sort range '{Range}' by column '{SortColumn}' in {(Ascending ? "ascending" : "descending")} order.", ex);
        }
    }
}