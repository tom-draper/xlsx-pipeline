using ClosedXML.Excel;

namespace XLSXPipeline.Actions;

internal class Validation
{
    public static void ValidateSheetExists(XLWorkbook workbook, string? newSheetName)
    {
        if (!workbook.Worksheets.Any(ws => ws.Name == newSheetName))
            throw new InvalidOperationException($"Sheet '{newSheetName}' does not exist in the target workbook.");
    }

    public static void ValidateSheetNotExists(XLWorkbook workbook, string? newSheetName)
    {
        if (workbook.Worksheets.Any(ws => ws.Name == newSheetName))
            throw new InvalidOperationException($"Sheet '{newSheetName}' already exists in the target workbook.");
    }

    public static void ValidateTargetIndex(XLWorkbook workbook, int targetIndex)
    {
        int maxIndex = workbook.Worksheets.Count + 1; // +1 because we might be adding a sheet
        if (targetIndex < 1 || targetIndex > maxIndex)
            throw new ArgumentOutOfRangeException(nameof(targetIndex),
                $"Target index must be between 1 and {maxIndex}.");
    }

    public static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
    }

    public static void ValidateColumnRange(int startColumn, int count, string sheetType)
    {
        int maxColumn = XLHelper.MaxColumnNumber;
        if (startColumn + count - 1 > maxColumn)
            throw new ArgumentOutOfRangeException(nameof(count),
                $"Cannot copy {count} columns starting from column {startColumn} in {sheetType} sheet. " +
                $"This would exceed the maximum column limit of {maxColumn}.");
    }

    public static void ValidateCell(IXLCell cell)
    {
        if (cell == null)
            throw new InvalidOperationException("Target cell could not be retrieved from the worksheet.");
    }
}
