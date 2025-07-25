using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class InsertColumnAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string ColumnName { get; set; }
    public int Count { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            InsertColumns(worksheet);

            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void InsertColumns(IXLWorksheet worksheet)
    {
        ValidateInputs();
        var targetColumn = GetTargetColumn(worksheet);
        ValidateColumnInsertion(targetColumn);
        PerformColumnInsertion(targetColumn);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(ColumnName))
            throw new ArgumentException("Column name cannot be null or empty.", nameof(ColumnName));

        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be greater than 0.");
    }

    private IXLColumn GetTargetColumn(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Column(ColumnName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Column '{ColumnName}' does not exist or is invalid.", ex);
        }
    }

    private void ValidateColumnInsertion(IXLColumn targetColumn)
    {
        int maxColumn = XLHelper.MaxColumnNumber;
        int targetColumnNumber = targetColumn.ColumnNumber();

        // Check if inserting columns would exceed the maximum column limit
        if (targetColumnNumber + Count > maxColumn)
            throw new ArgumentOutOfRangeException(nameof(Count),
                $"Cannot insert {Count} columns after column {targetColumnNumber}. " +
                $"This would exceed the maximum column limit of {maxColumn}.");
    }

    private void PerformColumnInsertion(IXLColumn targetColumn)
    {
        targetColumn.InsertColumnsAfter(Count);
    }
}