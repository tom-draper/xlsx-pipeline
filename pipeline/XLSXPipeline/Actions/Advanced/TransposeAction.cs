using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Advanced;

public class TransposeAction : ActionBase
{
    // Backing field for SourceSheetName
    private string? _sourceSheetName;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? SourceSheetName
    {
        get => _sourceSheetName != null ? Helpers.ReplaceDateTimePlaceholders(_sourceSheetName) : null;
        set => _sourceSheetName = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("sourceSheetName")]
    public string? JsonSourceSheetName
    {
        get => _sourceSheetName;
        set => _sourceSheetName = value;
    }
    public required string SourceRange { get; set; }
    // Backing field for DestinationSheetName
    private string? _destinationSheetName;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? DestinationSheetName
    {
        get => _destinationSheetName != null ? Helpers.ReplaceDateTimePlaceholders(_destinationSheetName) : null;
        set => _destinationSheetName = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("destinationSheetName")]
    public string? JsonDestinationSheetName
    {
        get => _destinationSheetName;
        set => _destinationSheetName = value;
    }
    public required string DestinationCell { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        TransposeData(workbook);

        workbook.Save();

        return Task.CompletedTask;
    }

    private void TransposeData(XLWorkbook workbook)
    {
        ValidateInputs();

        var sourceWorksheet = Helpers.GetWorksheetOrFirst(workbook, SourceSheetName);
        var sourceRange = GetSourceRange(sourceWorksheet);
        var destinationWorksheet = Helpers.GetOrCreateWorksheet(workbook, DestinationSheetName);
        var destinationCell = GetDestinationCell(destinationWorksheet);

        ValidateSourceData(sourceRange);
        ValidateDestinationSpace(destinationWorksheet, destinationCell, sourceRange);

        PerformTranspose(sourceRange, destinationWorksheet, destinationCell);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(SourceSheetName))
            throw new ArgumentException("Source sheet name cannot be null or empty.", nameof(SourceSheetName));

        if (string.IsNullOrWhiteSpace(SourceRange))
            throw new ArgumentException("Source range cannot be null or empty.", nameof(SourceRange));

        if (string.IsNullOrWhiteSpace(DestinationSheetName))
            throw new ArgumentException("Destination sheet name cannot be null or empty.", nameof(DestinationSheetName));

        if (string.IsNullOrWhiteSpace(DestinationCell))
            throw new ArgumentException("Destination cell cannot be null or empty.", nameof(DestinationCell));

        // Validate cell address format
        if (!IsValidCellAddress(DestinationCell))
            throw new ArgumentException($"Invalid destination cell address format '{DestinationCell}'. Please use a valid format (e.g., 'A1', 'B5').", nameof(DestinationCell));
    }

    private static bool IsValidCellAddress(string cellAddress)
    {
        try
        {
            return !string.IsNullOrWhiteSpace(cellAddress) &&
                   cellAddress.Length >= 2 &&
                   char.IsLetter(cellAddress[0]);
        }
        catch
        {
            return false;
        }
    }

    private IXLRange GetSourceRange(IXLWorksheet sourceWorksheet)
    {
        try
        {
            var range = sourceWorksheet.Range(SourceRange);
            if (range == null)
                throw new InvalidOperationException($"Failed to create range from '{SourceRange}'.");

            return range;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid source range specification '{SourceRange}'. Please ensure the range is valid (e.g., 'A1:D10').", ex);
        }
    }

    private IXLCell GetDestinationCell(IXLWorksheet destinationWorksheet)
    {
        try
        {
            return destinationWorksheet.Cell(DestinationCell);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to locate destination cell '{DestinationCell}' in worksheet '{DestinationSheetName}'.", ex);
        }
    }

    private static void ValidateSourceData(IXLRange sourceRange)
    {
        if (sourceRange.IsEmpty())
            throw new InvalidOperationException($"Source range '{sourceRange.RangeAddress}' contains no data to transpose.");

        var rowCount = sourceRange.RowCount();
        var columnCount = sourceRange.ColumnCount();

        if (rowCount < 1 || columnCount < 1)
            throw new InvalidOperationException($"Source range '{sourceRange.RangeAddress}' must contain at least one row and one column.");
    }

    private static void ValidateDestinationSpace(IXLWorksheet destinationWorksheet, IXLCell destinationCell, IXLRange sourceRange)
    {
        var sourceRowCount = sourceRange.RowCount();
        var sourceColumnCount = sourceRange.ColumnCount();

        // After transpose: source rows become columns, source columns become rows
        var destinationEndRow = destinationCell.Address.RowNumber + sourceColumnCount - 1;
        var destinationEndColumn = destinationCell.Address.ColumnNumber + sourceRowCount - 1;

        // Check if the transposed data will fit within Excel limits
        if (destinationEndRow > XLHelper.MaxRowNumber)
            throw new InvalidOperationException(
                $"Transposed data would extend to row {destinationEndRow}, exceeding Excel's maximum row limit of {XLHelper.MaxRowNumber}. " +
                $"Consider using a different destination cell or reducing the source data size.");

        if (destinationEndColumn > XLHelper.MaxColumnNumber)
            throw new InvalidOperationException(
                $"Transposed data would extend to column {destinationEndColumn}, exceeding Excel's maximum column limit of {XLHelper.MaxColumnNumber}. " +
                $"Consider using a different destination cell or reducing the source data size.");
    }

    private static void PerformTranspose(IXLRange sourceRange, IXLWorksheet destinationWorksheet, IXLCell destinationCell)
    {
        try
        {
            var sourceData = CaptureSourceData(sourceRange);
            WriteTransposedData(destinationWorksheet, destinationCell, sourceData);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to perform transpose operation.", ex);
        }
    }

    private static XLCellValue[,] CaptureSourceData(IXLRange sourceRange)
    {
        try
        {
            var rowCount = sourceRange.RowCount();
            var columnCount = sourceRange.ColumnCount();
            var sourceData = new XLCellValue[rowCount, columnCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < columnCount; c++)
                {
                    var sourceCell = sourceRange.Cell(r + 1, c + 1);
                    sourceData[r, c] = sourceCell.IsEmpty() ? XLCellValue.FromObject(null) : sourceCell.Value;
                }
            }

            return sourceData;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to capture source data for transpose operation.", ex);
        }
    }

    private static void WriteTransposedData(IXLWorksheet destinationWorksheet, IXLCell destinationCell, XLCellValue[,] sourceData)
    {
        try
        {
            var sourceRowCount = sourceData.GetLength(0);
            var sourceColumnCount = sourceData.GetLength(1);
            var destRowStart = destinationCell.Address.RowNumber;
            var destColumnStart = destinationCell.Address.ColumnNumber;

            // Transpose: source[r,c] becomes destination[c,r]
            for (int r = 0; r < sourceRowCount; r++)
            {
                for (int c = 0; c < sourceColumnCount; c++)
                {
                    var sourceValue = sourceData[r, c];

                    // Skip empty cells to avoid overwriting existing data unnecessarily
                    if (!sourceValue.IsBlank)
                    {
                        // Transposed position: swap row and column indices
                        var destRow = destRowStart + c;
                        var destColumn = destColumnStart + r;

                        destinationWorksheet.Cell(destRow, destColumn).Value = sourceValue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to write transposed data to destination starting at cell '{destinationCell.Address}'. " +
                "Ensure there is sufficient space and no conflicting data.", ex);
        }
    }
}