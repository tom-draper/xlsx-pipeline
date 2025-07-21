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
            var worksheet = string.IsNullOrEmpty(SheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(SheetName);

            // Get the range of rows to move
            var rowsToMove = worksheet.Rows(FromRow, FromRow + Count - 1);

            // Copy the rows to a temporary location
            var lastColumn = worksheet.RangeUsed()?.LastColumn()?.ColumnNumber() ?? worksheet.ColumnCount();
            var tempRange = worksheet.Range(FromRow, 1, FromRow + Count - 1, lastColumn);
            var tempData = new object[Count, tempRange.ColumnCount()];

            // Store the data
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < tempRange.ColumnCount(); j++)
                    tempData[i, j] = worksheet.Cell(FromRow + i, j + 1).Value;
            }

            // Delete the original rows
            rowsToMove.Delete();

            // Adjust target row if it's after the deleted rows
            int adjustedToRow = ToRow > FromRow ? ToRow - Count : ToRow;

            // Insert new rows at the target location
            worksheet.Row(adjustedToRow).InsertRowsAbove(Count);

            // Copy the data to the new location
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < tempRange.ColumnCount(); j++)
                    worksheet.Cell(adjustedToRow + i, j + 1).Value = (XLCellValue)tempData[i, j];
            }

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}