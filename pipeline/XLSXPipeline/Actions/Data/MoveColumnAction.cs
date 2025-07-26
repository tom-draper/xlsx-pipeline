using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class MoveColumnAction : ActionBase
{
    [ReplacePlaceholders]
    public string? SheetName { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            MoveColumn(worksheet);

            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void MoveColumn(IXLWorksheet worksheet)
    {
        ValidateInputs();
        var sourceColumn = GetColumn(worksheet, From);
        var destinationColumn = GetColumn(worksheet, To);

        ValidateMoveOperation(sourceColumn, destinationColumn);
        PerformColumnMove(sourceColumn, destinationColumn);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(From))
            throw new ArgumentException("From column cannot be null or empty.", nameof(From));

        if (string.IsNullOrWhiteSpace(To))
            throw new ArgumentException("To column cannot be null or empty.", nameof(To));

        if (string.Equals(From.Trim(), To.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Source and destination columns cannot be the same.", nameof(To));
    }

    private static IXLColumn GetColumn(IXLWorksheet worksheet, string column)
    {
        try
        {
            int sourceIndex = XLHelper.GetColumnNumberFromLetter(column);
            return worksheet.Column(sourceIndex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Column '{column}' does not exist or is invalid.", ex);
        }
    }

    private static void ValidateMoveOperation(IXLColumn sourceColumn, IXLColumn destinationColumn)
    {
        // Verify columns are from the same worksheet
        if (!ReferenceEquals(sourceColumn.Worksheet, destinationColumn.Worksheet))
            throw new InvalidOperationException("Source and destination columns must be from the same worksheet.");
    }

    private static void PerformColumnMove(IXLColumn sourceColumn, IXLColumn destinationColumn)
    {
        var ws = sourceColumn.Worksheet;
        int sourceIndex = sourceColumn.ColumnNumber();
        int destIndex = destinationColumn.ColumnNumber();

        // Insert a new column at the destination to make space
        ws.Column(destIndex).InsertColumnsBefore(1);

        // Adjust destination index if source is before destination
        if (sourceIndex < destIndex)
            destIndex--; // Because insert shifted the original destination right

        // Copy contents & formatting
        ws.Column(sourceIndex).CopyTo(ws.Column(destIndex));

        // Delete the old source column
        if (sourceIndex > destIndex)
            ws.Column(sourceIndex + 1).Delete(); // it was shifted right
        else
            ws.Column(sourceIndex).Delete(); // safe if already adjusted
    }
}