using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class DeduplicateRowsAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to deduplicate. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Columns to compare when determining duplicates, e.g. ["A", "C", "E"].
    /// Null or empty means all used columns are compared.
    /// </summary>
    public List<string>? Columns { get; set; }

    /// <summary>
    /// When true, the first row is treated as a header and is never removed. Defaults to true.
    /// </summary>
    public bool HasHeaders { get; set; } = true;

    /// <summary>
    /// When true, the first occurrence of a duplicate group is kept and subsequent ones removed.
    /// When false, the last occurrence is kept. Defaults to true.
    /// </summary>
    public bool KeepFirst { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        var usedRange = worksheet.RangeUsed();
        if (usedRange == null)
            return Task.CompletedTask;

        int firstDataRow = HasHeaders ? usedRange.FirstRow().RowNumber() + 1 : usedRange.FirstRow().RowNumber();
        int lastRow = usedRange.LastRow().RowNumber();
        int firstCol = usedRange.FirstColumn().ColumnNumber();
        int lastCol = usedRange.LastColumn().ColumnNumber();

        var keyColumns = ResolveKeyColumns(worksheet, firstCol, lastCol);

        var seen = new HashSet<string>();
        var rowsToDelete = new List<int>();

        // Iterate forwards; track which rows are duplicates
        for (int rowNum = firstDataRow; rowNum <= lastRow; rowNum++)
        {
            var key = BuildRowKey(worksheet, rowNum, keyColumns);
            bool isNew = seen.Add(key);

            if (KeepFirst && !isNew)
                rowsToDelete.Add(rowNum);
        }

        // When keeping last, we need a second pass
        if (!KeepFirst)
        {
            seen.Clear();
            for (int rowNum = lastRow; rowNum >= firstDataRow; rowNum--)
            {
                var key = BuildRowKey(worksheet, rowNum, keyColumns);
                bool isNew = seen.Add(key);
                if (!isNew)
                    rowsToDelete.Add(rowNum);
            }
        }

        // Delete in reverse order to preserve row numbers during deletion
        rowsToDelete.Sort();
        for (int i = rowsToDelete.Count - 1; i >= 0; i--)
            worksheet.Row(rowsToDelete[i]).Delete();

        workbook.Save();
        return Task.CompletedTask;
    }

    private List<int> ResolveKeyColumns(IXLWorksheet worksheet, int firstCol, int lastCol)
    {
        if (Columns == null || Columns.Count == 0)
            return Enumerable.Range(firstCol, lastCol - firstCol + 1).ToList();

        return Columns.Select(col =>
            int.TryParse(col, out int colNum) ? colNum : worksheet.Column(col).ColumnNumber()
        ).ToList();
    }

    private static string BuildRowKey(IXLWorksheet worksheet, int rowNum, List<int> keyColumns)
    {
        var parts = keyColumns.Select(col => worksheet.Cell(rowNum, col).GetString());
        return string.Join('\0', parts);
    }
}
