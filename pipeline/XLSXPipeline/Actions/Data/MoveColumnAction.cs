using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class MoveColumnAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = GetWorksheet(workbook, SheetName);
            MoveColumn(worksheet);
            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string? sheetName)
    {
        if (string.IsNullOrEmpty(sheetName))
        {
            if (workbook.Worksheets.Count == 0)
                throw new InvalidOperationException("No worksheets found in the workbook.");
            return workbook.Worksheets.First();
        }

        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Worksheet '{sheetName}' does not exist.");

        return worksheet;
    }

    private void MoveColumn(IXLWorksheet worksheet)
    {
        ValidateInputs();
        var sourceColumn = GetSourceColumn(worksheet);
        var destinationColumn = GetDestinationColumn(worksheet);

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

    private IXLColumn GetSourceColumn(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Column(From);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Source column '{From}' does not exist or is invalid.", ex);
        }
    }

    private IXLColumn GetDestinationColumn(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Column(To);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Destination column '{To}' does not exist or is invalid.", ex);
        }
    }

    private static void ValidateMoveOperation(IXLColumn sourceColumn, IXLColumn destinationColumn)
    {
        // Verify columns are from the same worksheet
        if (!ReferenceEquals(sourceColumn.Worksheet, destinationColumn.Worksheet))
            throw new InvalidOperationException("Source and destination columns must be from the same worksheet.");

        // Check if source column has any data to move
        var sourceUsedRange = sourceColumn.Worksheet.RangeUsed();
        if (sourceUsedRange != null)
        {
            int sourceColumnNumber = sourceColumn.ColumnNumber();
            int firstUsedColumn = sourceUsedRange.FirstColumn().ColumnNumber();
            int lastUsedColumn = sourceUsedRange.LastColumn().ColumnNumber();

            // Only validate if the source column is within the used range
            if (sourceColumnNumber >= firstUsedColumn && sourceColumnNumber <= lastUsedColumn)
            {
                // Check if there's actually data in the source column
                var sourceColumnRange = sourceColumn.Worksheet.Column(sourceColumnNumber).CellsUsed();
                if (!sourceColumnRange.Any())
                {
                    // Source column is empty, but this is not an error - just a no-op
                }
            }
        }
    }

    private static void PerformColumnMove(IXLColumn sourceColumn, IXLColumn destinationColumn)
    {
        try
        {
            // Copy the source column to the destination
            sourceColumn.CopyTo(destinationColumn);

            // Delete the source column
            sourceColumn.Delete();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to move column from '{sourceColumn.ColumnLetter()}' to '{destinationColumn.ColumnLetter()}'.", ex);
        }
    }
}