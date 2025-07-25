using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class MoveRowAction : ActionBase
{
    public string? SheetName { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            MoveRows(worksheet);

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void MoveRows(IXLWorksheet worksheet)
    {
        ValidateInputs();
        ValidateRowRange(worksheet);

        var sourceRange = GetSourceRowRange(worksheet);
        var rowSnapshots = CaptureRowSnapshots(sourceRange);

        int adjustedTo = CalculateAdjustedTargetRow();

        DeleteSourceRows(worksheet);
        InsertRowsAtTarget(worksheet, adjustedTo);
        RestoreRowSnapshots(worksheet, rowSnapshots, adjustedTo);
    }

    private void ValidateInputs()
    {
        if (From < 1)
            throw new ArgumentOutOfRangeException(nameof(From), "From row must be greater than 0.");
        if (To < 1)
            throw new ArgumentOutOfRangeException(nameof(To), "To row must be greater than 0.");
        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be greater than 0.");
        if (From == To)
            throw new ArgumentException("Source and destination rows cannot be the same.", nameof(To));
    }

    private void ValidateRowRange(IXLWorksheet worksheet)
    {
        int maxRow = XLHelper.MaxRowNumber;

        if (From + Count - 1 > maxRow)
            throw new ArgumentOutOfRangeException(nameof(Count),
                $"Cannot move {Count} rows starting from row {From}. This would exceed Excel's row limit ({maxRow}).");

        int adjustedTo = CalculateAdjustedTargetRow();
        if (adjustedTo + Count - 1 > maxRow)
            throw new ArgumentOutOfRangeException(nameof(To),
                $"Cannot move rows to row {adjustedTo}. This would exceed Excel's row limit ({maxRow}).");

        int sourceEnd = From + Count - 1;
        int destEnd = To + Count - 1;
        if ((From <= To && To <= sourceEnd) || (To <= From && From <= destEnd))
            throw new ArgumentException("Source and destination row ranges cannot overlap.", nameof(To));
    }

    private IXLRange GetSourceRowRange(IXLWorksheet worksheet)
    {
        int lastCol = worksheet.RangeUsed()?.LastColumn()?.ColumnNumber() ?? 1;
        return worksheet.Range(From, 1, From + Count - 1, lastCol);
    }

    private int CalculateAdjustedTargetRow()
    {
        // When moving down, we need to adjust the insertion point because source rows are deleted first
        return To > From ? To - Count : To;
    }

    private void DeleteSourceRows(IXLWorksheet worksheet)
    {
        worksheet.Rows(From, From + Count - 1).Delete();
    }

    private void InsertRowsAtTarget(IXLWorksheet worksheet, int targetRow)
    {
        worksheet.Row(targetRow).InsertRowsAbove(Count);
    }

    private record CellSnapshot(XLCellValue Value, string? Formula, IXLStyle Style, bool IsMerged);

    private static CellSnapshot[,] CaptureRowSnapshots(IXLRange sourceRange)
    {
        int rowCount = sourceRange.RowCount();
        int colCount = sourceRange.ColumnCount();
        var snapshots = new CellSnapshot[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            var row = sourceRange.Row(i + 1);
            for (int j = 0; j < colCount; j++)
            {
                var cell = row.Cell(j + 1);
                snapshots[i, j] = new CellSnapshot(
                    cell.Value,
                    cell.HasFormula ? cell.FormulaA1 : null,
                    cell.Style,
                    cell.IsMerged()
                );
            }
        }

        return snapshots;
    }

    private void RestoreRowSnapshots(IXLWorksheet worksheet, CellSnapshot[,] snapshots, int targetRow)
    {
        int rowCount = snapshots.GetLength(0);
        int colCount = snapshots.GetLength(1);

        for (int i = 0; i < rowCount; i++)
        {
            var row = worksheet.Row(targetRow + i);
            for (int j = 0; j < colCount; j++)
            {
                var cell = worksheet.Cell(targetRow + i, j + 1);
                var snap = snapshots[i, j];

                if (snap.Formula != null)
                    cell.FormulaA1 = snap.Formula;
                else
                    cell.Value = snap.Value;

                cell.Style = snap.Style;

                // Optional: you could re-merge if needed
                // Note: This simple logic does not restore merged ranges; use more advanced tracking if needed.
            }

            // Preserve row height
            row.Height = worksheet.Row(From + i).Height;
        }
    }
}
