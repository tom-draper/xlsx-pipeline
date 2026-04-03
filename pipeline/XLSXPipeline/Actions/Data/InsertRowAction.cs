using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class InsertRowAction : ActionBase
{
    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    public int RowNumber { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        InsertRows(worksheet);

        workbook.Save();

        return Task.CompletedTask;
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

    private IXLRow GetTargetRow(IXLWorksheet worksheet) => worksheet.Row(RowNumber);

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