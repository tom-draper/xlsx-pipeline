using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class InsertRowAction : ActionBase
{
    public string? SheetName { get; set; }
    public int RowNumber { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = GetWorksheet(workbook, SheetName);
            InsertRows(worksheet);
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

    private void InsertRows(IXLWorksheet worksheet)
    {
        ValidateInputs();
        var targetRow = GetTargetRow(worksheet);
        ValidateRowInsertion(targetRow);
        PerformRowInsertion(targetRow);
    }

    private void ValidateInputs()
    {
        if (RowNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(RowNumber), "Row number must be greater than 0.");

        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be greater than 0.");
    }

    private IXLRow GetTargetRow(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Row(RowNumber);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Row {RowNumber} does not exist or is invalid.", ex);
        }
    }

    private void ValidateRowInsertion(IXLRow targetRow)
    {
        int maxRow = XLHelper.MaxRowNumber;
        int targetRowNumber = targetRow.RowNumber();

        // Check if inserting rows would exceed the maximum row limit
        if (targetRowNumber + Count > maxRow)
            throw new ArgumentOutOfRangeException(nameof(Count),
                $"Cannot insert {Count} rows above row {targetRowNumber}. " +
                $"This would exceed the maximum row limit of {maxRow}.");
    }

    private void PerformRowInsertion(IXLRow targetRow)
    {
        targetRow.InsertRowsAbove(Count);
    }
}