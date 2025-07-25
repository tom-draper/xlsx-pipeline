using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class MergeDataAction : ActionBase
{
    public required string SourceFilePath { get; set; }
    public string? SourceSheetName { get; set; }
    public string? DestinationSheetName { get; set; }
    public string DestinationCell { get; set; } = "A1";
    public bool IncludeHeaders { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var destWorkbook = new XLWorkbook(filePath);
            using var sourceWorkbook = new XLWorkbook(SourceFilePath);

            var sourceSheet = Helpers.GetWorksheetOrFirst(sourceWorkbook, SourceSheetName);
            var destSheet = Helpers.GetWorksheetOrFirst(destWorkbook, DestinationSheetName);

            MergeData(sourceSheet, destSheet);

            destWorkbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void MergeData(IXLWorksheet sourceSheet, IXLWorksheet destSheet)
    {
        ValidateInputs();
        var usedRange = GetSourceDataRange(sourceSheet);

        if (usedRange == null)
            return; // No data to merge

        var dataRange = DetermineDataRange(sourceSheet, usedRange);
        var destinationCell = GetDestinationCell(destSheet);

        ValidateMergeOperation(dataRange, destinationCell, destSheet);
        PerformDataMerge(dataRange, destinationCell);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(SourceFilePath))
            throw new ArgumentException("Source file path cannot be null or empty.", nameof(SourceFilePath));

        if (!System.IO.File.Exists(SourceFilePath))
            throw new FileNotFoundException($"Source file '{SourceFilePath}' does not exist.", SourceFilePath);

        if (string.IsNullOrWhiteSpace(DestinationCell))
            throw new ArgumentException("Destination cell cannot be null or empty.", nameof(DestinationCell));
    }

    private static IXLRange? GetSourceDataRange(IXLWorksheet sourceSheet)
    {
        try
        {
            return sourceSheet.RangeUsed();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to determine used range in source sheet.", ex);
        }
    }

    private IXLRange DetermineDataRange(IXLWorksheet sourceSheet, IXLRange usedRange)
    {
        var startRow = IncludeHeaders ? 1 : 2;
        var endRow = usedRange.LastRow().RowNumber();
        var endColumn = usedRange.LastColumn().ColumnNumber();

        if (startRow > endRow)
            throw new InvalidOperationException("No data rows available after excluding headers.");

        try
        {
            return sourceSheet.Range(startRow, 1, endRow, endColumn);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create data range from row {startRow} to row {endRow}.", ex);
        }
    }

    private IXLCell GetDestinationCell(IXLWorksheet destSheet)
    {
        try
        {
            return destSheet.Cell(DestinationCell);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid destination cell reference '{DestinationCell}'.", ex);
        }
    }

    private static void ValidateMergeOperation(IXLRange dataRange, IXLCell destinationCell, IXLWorksheet destSheet)
    {
        int sourceRowCount = dataRange.RowCount();
        int sourceColumnCount = dataRange.ColumnCount();
        int destStartRow = destinationCell.Address.RowNumber;
        int destStartColumn = destinationCell.Address.ColumnNumber;

        // Check if the merge would exceed worksheet boundaries
        int maxRow = XLHelper.MaxRowNumber;
        int maxColumn = XLHelper.MaxColumnNumber;

        if (destStartRow + sourceRowCount - 1 > maxRow)
            throw new InvalidOperationException(
                $"Cannot merge {sourceRowCount} rows starting at row {destStartRow}. " +
                $"This would exceed the maximum row limit of {maxRow}.");

        if (destStartColumn + sourceColumnCount - 1 > maxColumn)
            throw new InvalidOperationException(
                $"Cannot merge {sourceColumnCount} columns starting at column {destStartColumn}. " +
                $"This would exceed the maximum column limit of {maxColumn}.");
    }

    private static void PerformDataMerge(IXLRange dataRange, IXLCell destinationCell)
    {
        try
        {
            dataRange.CopyTo(destinationCell);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to copy data from source to destination.", ex);
        }
    }
}