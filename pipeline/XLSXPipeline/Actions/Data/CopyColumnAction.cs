using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class CopyColumnAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string SourceColumn { get; set; }
    public required string DestinationColumn { get; set; }
    public int Count { get; set; } = 1;
    public string? DestinationSheetName { get; set; }
    public bool InsertColumns { get; set; } = false; // If true, insert new columns; if false, overwrite existing

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var sourceSheet = GetSourceSheet(workbook, SheetName);
            var destSheet = GetOrCreateDestinationSheet(workbook, DestinationSheetName, sourceSheet);

            CopyColumns(sourceSheet, destSheet);

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

    private void CopyColumns(IXLWorksheet sourceSheet, IXLWorksheet destSheet)
    {
        var sourceColumnNumber = GetColumnNumber(sourceSheet, SourceColumn, "Source");
        var destColumnNumber = GetColumnNumber(destSheet, DestinationColumn, "Destination");

        ValidateColumnRange(sourceSheet, sourceColumnNumber, Count, "source");

        if (InsertColumns)
            InsertColumnsAtDestination(destSheet, destColumnNumber);

        CopyColumnRange(sourceSheet, destSheet, sourceColumnNumber, destColumnNumber);
    }

    private static int GetColumnNumber(IXLWorksheet sheet, string columnReference, string columnType)
    {
        try
        {
            return sheet.Column(columnReference).ColumnNumber();
        }
        catch
        {
            throw new InvalidOperationException($"{columnType} column '{columnReference}' is not valid.");
        }
    }

    private static void ValidateColumnRange(IXLWorksheet sheet, int startColumn, int count, string sheetType)
    {
        int maxColumn = XLHelper.MaxColumnNumber;
        if (startColumn + count - 1 > maxColumn)
            throw new ArgumentOutOfRangeException(nameof(count),
                $"Cannot copy {count} columns starting from column {startColumn} in {sheetType} sheet. " +
                $"This would exceed the maximum column limit of {maxColumn}.");
    }

    private void InsertColumnsAtDestination(IXLWorksheet destSheet, int destColumnNumber)
    {
        if (Count > 1)
            destSheet.Column(destColumnNumber).InsertColumnsAfter(Count - 1);
    }

    private void CopyColumnRange(IXLWorksheet sourceSheet, IXLWorksheet destSheet, int sourceColumnNumber, int destColumnNumber)
    {
        for (int i = 0; i < Count; i++)
        {
            var sourceCol = sourceSheet.Column(sourceColumnNumber + i);
            var destCol = destSheet.Column(destColumnNumber + i);
            sourceCol.CopyTo(destCol);
        }
    }
}