using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class DeleteRowAction : ActionBase
{
    [ReplacePlaceholders]
    public string? SheetName { get; set; }
    public int RowNumber { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            DeleteRows(worksheet);

            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void DeleteRows(IXLWorksheet worksheet)
    {
        ValidateInputs();
        ValidateRowRange(worksheet);
        PerformRowDeletion(worksheet);
    }

    private void ValidateInputs()
    {
        if (RowNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(RowNumber), "Row number must be greater than 0.");

        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be greater than 0.");
    }

    private void ValidateRowRange(IXLWorksheet worksheet)
    {
        int maxRow = XLHelper.MaxRowNumber;

        if (RowNumber + Count - 1 > maxRow)
            throw new ArgumentOutOfRangeException(nameof(Count),
                $"Cannot delete {Count} rows starting from row {RowNumber}. " +
                $"This would exceed the maximum row limit of {maxRow}.");

        // Check if we're trying to delete more rows than exist with data
        var lastUsedRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastUsedRow > 0 && RowNumber > lastUsedRow)
            throw new InvalidOperationException(
                $"Cannot delete row {RowNumber}. The worksheet only has data up to row {lastUsedRow}.");
    }

    private void PerformRowDeletion(IXLWorksheet worksheet)
    {
        int endRowNumber = RowNumber + Count - 1;
        worksheet.Rows(RowNumber, endRowNumber).Delete();
    }
}