using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class CopyRowAction : ActionBase
{
    public string? SheetName { get; set; }
    public int SourceRow { get; set; }
    public int DestinationRow { get; set; }
    public int Count { get; set; } = 1;
    public string? DestinationSheetName { get; set; }
    public bool InsertRows { get; set; } = false; // If true, insert new rows; if false, overwrite existing

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var sourceSheet = GetSourceSheet(workbook, SheetName);
            var destSheet = GetOrCreateDestinationSheet(workbook, DestinationSheetName, sourceSheet);

            CopyRows(sourceSheet, destSheet);

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private static IXLWorksheet GetSourceSheet(XLWorkbook workbook, string? sheetName)
    {
        if (string.IsNullOrEmpty(sheetName))
        {
            if (workbook.Worksheets.Count == 0)
                throw new InvalidOperationException("No worksheets found in the workbook.");
            return workbook.Worksheets.First();
        }

        var sheet = workbook.Worksheet(sheetName);
        if (sheet == null)
            throw new InvalidOperationException($"Source sheet '{sheetName}' does not exist.");

        return sheet;
    }

    private static IXLWorksheet GetOrCreateDestinationSheet(XLWorkbook workbook, string? destinationSheetName, IXLWorksheet sourceSheet)
    {
        if (string.IsNullOrEmpty(destinationSheetName))
            return sourceSheet;

        var destSheet = workbook.Worksheet(destinationSheetName);
        if (destSheet == null)
            destSheet = workbook.Worksheets.Add(destinationSheetName);

        return destSheet;
    }

    private void CopyRows(IXLWorksheet sourceSheet, IXLWorksheet destSheet)
    {
        ValidateRowNumbers();
        ValidateRowRange(sourceSheet, SourceRow, Count, "source");
        ValidateRowRange(destSheet, DestinationRow, Count, "destination");

        if (InsertRows)
            InsertRowsAtDestination(destSheet, DestinationRow);

        CopyRowRange(sourceSheet, destSheet, SourceRow, DestinationRow);
    }

    private void ValidateRowNumbers()
    {
        if (SourceRow < 1)
            throw new ArgumentOutOfRangeException(nameof(SourceRow), "Source row must be greater than 0.");

        if (DestinationRow < 1)
            throw new ArgumentOutOfRangeException(nameof(DestinationRow), "Destination row must be greater than 0.");

        if (Count < 1)
            throw new ArgumentOutOfRangeException(nameof(Count), "Count must be greater than 0.");
    }

    private static void ValidateRowRange(IXLWorksheet sheet, int startRow, int count, string sheetType)
    {
        int maxRow = XLHelper.MaxRowNumber;
        if (startRow + count - 1 > maxRow)
            throw new ArgumentOutOfRangeException(nameof(count),
                $"Cannot copy {count} rows starting from row {startRow} in {sheetType} sheet. " +
                $"This would exceed the maximum row limit of {maxRow}.");
    }

    private void InsertRowsAtDestination(IXLWorksheet destSheet, int destinationRow)
    {
        if (Count > 0)
            destSheet.Row(destinationRow).InsertRowsAbove(Count);
    }

    private void CopyRowRange(IXLWorksheet sourceSheet, IXLWorksheet destSheet, int sourceRow, int destinationRow)
    {
        for (int i = 0; i < Count; i++)
        {
            var sourceRowRange = sourceSheet.Row(sourceRow + i);
            var destRowRange = destSheet.Row(destinationRow + i);
            sourceRowRange.CopyTo(destRowRange);
        }
    }
}