using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class MoveRowAction : ActionBase
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
        var rowSnapshots = CaptureRowSnapshots(sourceRange, worksheet);

        int adjustedTo = CalculateAdjustedTargetRow();

        DeleteSourceRows(worksheet);
        InsertRowsAtTarget(worksheet, adjustedTo);
        RestoreRowSnapshots(worksheet, rowSnapshots, To);
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

        if (To + Count - 1 > maxRow)
            throw new ArgumentOutOfRangeException(nameof(To),
                $"Cannot move rows to row {To}. This would exceed Excel's row limit ({maxRow}).");

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

    private record RowSnapshot(CellSnapshot[] Cells, double Height);

    private static RowSnapshot[] CaptureRowSnapshots(IXLRange sourceRange, IXLWorksheet worksheet)
    {
        int rowCount = sourceRange.RowCount();
        int colCount = sourceRange.ColumnCount();
        var snapshots = new RowSnapshot[rowCount];

        for (int i = 0; i < rowCount; i++)
        {
            var row = sourceRange.Row(i + 1);
            var worksheetRow = worksheet.Row(sourceRange.FirstCell().Address.RowNumber + i);
            var cells = new CellSnapshot[colCount];

            for (int j = 0; j < colCount; j++)
            {
                var cell = row.Cell(j + 1);
                cells[j] = new CellSnapshot(
                    cell.Value,
                    cell.HasFormula ? cell.FormulaA1 : null,
                    cell.Style,
                    cell.IsMerged()
                );
            }

            snapshots[i] = new RowSnapshot(cells, worksheetRow.Height);
        }

        return snapshots;
    }

    private static void RestoreRowSnapshots(IXLWorksheet worksheet, RowSnapshot[] snapshots, int targetRow)
    {
        for (int i = 0; i < snapshots.Length; i++)
        {
            var rowSnapshot = snapshots[i];
            var row = worksheet.Row(targetRow + i);

            // Restore row height
            row.Height = rowSnapshot.Height;

            // Restore cells
            for (int j = 0; j < rowSnapshot.Cells.Length; j++)
            {
                var cell = worksheet.Cell(targetRow + i, j + 1);
                var cellSnapshot = rowSnapshot.Cells[j];

                if (cellSnapshot.Formula != null)
                    cell.FormulaA1 = cellSnapshot.Formula;
                else
                    cell.Value = cellSnapshot.Value;

                cell.Style = cellSnapshot.Style;

                // Optional: you could re-merge if needed
                // Note: This simple logic does not restore merged ranges; use more advanced tracking if needed.
            }
        }
    }
}
