using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class MoveRowAction : ActionBase
{
    public string? SheetName { get; set; }
    public int FromRow { get; set; }
    public int ToRow { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = GetWorksheet(workbook, SheetName);
            MoveRows(worksheet);
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

    private void MoveRows(IXLWorksheet worksheet)
    {
        ValidateInputs();
        ValidateRowRange(worksheet);

        var sourceRange = GetSourceRowRange(worksheet);
        var tempData = CaptureRowData(worksheet, sourceRange);
        var adjustedToRow = CalculateAdjustedTargetRow();

        DeleteSourceRows(worksheet);
        InsertRowsAtTarget(worksheet, adjustedToRow);
        RestoreRowData(worksheet, tempData, adjustedToRow);
    }

    private void ValidateInputs()
    {
        if (FromRow < 1)
            throw new ArgumentOutOfRangeException(nameof(FromRow), "From row must be greater than 0.");

        if (ToRow < 1)
            throw new ArgumentOutOfRangeException(nameof(ToRow), "To row must be greater than 0.");

        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be greater than 0.");

        if (FromRow == ToRow)
            throw new ArgumentException("Source and destination rows cannot be the same.", nameof(ToRow));
    }

    private void ValidateRowRange(IXLWorksheet worksheet)
    {
        int maxRow = XLHelper.MaxRowNumber;

        // Validate source range
        if (FromRow + Count - 1 > maxRow)
            throw new ArgumentOutOfRangeException(nameof(Count),
                $"Cannot move {Count} rows starting from row {FromRow}. " +
                $"This would exceed the maximum row limit of {maxRow}.");

        // Validate destination range
        int adjustedToRow = CalculateAdjustedTargetRow();
        if (adjustedToRow + Count - 1 > maxRow)
            throw new ArgumentOutOfRangeException(nameof(ToRow),
                $"Cannot move {Count} rows to row {adjustedToRow}. " +
                $"This would exceed the maximum row limit of {maxRow}.");

        // Check for overlapping ranges
        int sourceEnd = FromRow + Count - 1;
        int destEnd = ToRow + Count - 1;

        if ((FromRow <= ToRow && ToRow <= sourceEnd) || (ToRow <= FromRow && FromRow <= destEnd))
            throw new ArgumentException("Source and destination row ranges cannot overlap.", nameof(ToRow));
    }

    private IXLRange GetSourceRowRange(IXLWorksheet worksheet)
    {
        try
        {
            var lastColumn = worksheet.RangeUsed()?.LastColumn()?.ColumnNumber() ?? 1;
            return worksheet.Range(FromRow, 1, FromRow + Count - 1, lastColumn);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to determine source row range starting at row {FromRow}.", ex);
        }
    }

    private static XLCellValue[,] CaptureRowData(IXLWorksheet worksheet, IXLRange sourceRange)
    {
        try
        {
            int rowCount = sourceRange.RowCount();
            int columnCount = sourceRange.ColumnCount();
            var tempData = new XLCellValue[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    var cell = sourceRange.Cell(i + 1, j + 1);
                    tempData[i, j] = cell.Value;
                }
            }

            return tempData;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to capture row data for move operation.", ex);
        }
    }

    private int CalculateAdjustedTargetRow()
    {
        // If moving rows to a position after the source, adjust for the deletion
        return ToRow > FromRow ? ToRow - Count : ToRow;
    }

    private void DeleteSourceRows(IXLWorksheet worksheet)
    {
        try
        {
            var rowsToDelete = worksheet.Rows(FromRow, FromRow + Count - 1);
            rowsToDelete.Delete();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete source rows {FromRow} to {FromRow + Count - 1}.", ex);
        }
    }

    private void InsertRowsAtTarget(IXLWorksheet worksheet, int targetRow)
    {
        try
        {
            worksheet.Row(targetRow).InsertRowsAbove(Count);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to insert {Count} rows at row {targetRow}.", ex);
        }
    }

    private static void RestoreRowData(IXLWorksheet worksheet, XLCellValue[,] tempData, int targetRow)
    {
        try
        {
            int rowCount = tempData.GetLength(0);
            int columnCount = tempData.GetLength(1);

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                    worksheet.Cell(targetRow + i, j + 1).Value = tempData[i, j];
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to restore row data at target row {targetRow}.", ex);
        }
    }
}