using ClosedXML.Excel;

namespace XLSXPipeline.Tests.Infrastructure;

// Utility class for Excel file operations
public static class ExcelTestHelpers
{
    public static void CreateTestFile(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        // Add diverse test data
        worksheet.Cell("A1").Value = "Test Data";
        worksheet.Cell("B1").Value = "More Data";
        worksheet.Cell("A2").Value = 123;
        worksheet.Cell("B2").Value = 456.789;
        worksheet.Cell("A3").Value = DateTime.Now;
        worksheet.Cell("B3").Value = true;
        workbook.SaveAs(path);
    }

    public static void CreateTestFileWithData(string path, Dictionary<string, object> cellData)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        foreach (var kvp in cellData)
            worksheet.Cell(kvp.Key).Value = (XLCellValue)kvp.Value;
        workbook.SaveAs(path);
    }

    public static void VerifyFilesAreIdentical(string inputPath, string outputPath)
    {
        using var inputWorkbook = new XLWorkbook(inputPath);
        using var outputWorkbook = new XLWorkbook(outputPath);
        Assert.Equal(inputWorkbook.Worksheets.Count, outputWorkbook.Worksheets.Count);
        for (int i = 1; i <= inputWorkbook.Worksheets.Count; i++)
        {
            var inputSheet = inputWorkbook.Worksheet(i);
            var outputSheet = outputWorkbook.Worksheet(i);
            Assert.Equal(inputSheet.Name, outputSheet.Name);
            VerifyWorksheetsAreIdentical(inputSheet, outputSheet);
        }
    }

    /// <summary>
    /// Verifies that two rows in the same worksheet have identical values
    /// </summary>
    /// <param name="worksheet">The worksheet to check</param>
    /// <param name="rowIndex1">First row index to compare</param>
    /// <param name="rowIndex2">Second row index to compare</param>
    /// <param name="startColumn">Optional: Starting column index (default: 1)</param>
    /// <param name="endColumn">Optional: Ending column index (default: last used column)</param>
    public static void VerifyRowsAreIdentical(IXLWorksheet worksheet, int rowIndex1, int rowIndex2,
        int? startColumn = null, int? endColumn = null)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null)
            return; // No data in worksheet, rows are considered identical

        int firstCol = startColumn ?? usedRange.FirstColumn().ColumnNumber();
        int lastCol = endColumn ?? usedRange.LastColumn().ColumnNumber();

        for (int col = firstCol; col <= lastCol; col++)
        {
            var cell1 = worksheet.Cell(rowIndex1, col);
            var cell2 = worksheet.Cell(rowIndex2, col);

            Assert.Equal(cell1.GetString(), cell2.GetString());

            // Also verify formulas if present
            if (!string.IsNullOrEmpty(cell1.FormulaA1))
                Assert.Equal(cell1.FormulaA1, cell2.FormulaA1);
            else if (!string.IsNullOrEmpty(cell2.FormulaA1))
                Assert.Fail($"Row {rowIndex1} has no formula at column {col}, but row {rowIndex2} has formula: {cell2.FormulaA1}");
        }
    }

    /// <summary>
    /// Verifies that two columns in the same worksheet have identical values
    /// </summary>
    /// <param name="worksheet">The worksheet to check</param>
    /// <param name="columnIndex1">First column index to compare</param>
    /// <param name="columnIndex2">Second column index to compare</param>
    /// <param name="startRow">Optional: Starting row index (default: 1)</param>
    /// <param name="endRow">Optional: Ending row index (default: last used row)</param>
    public static void VerifyColumnsAreIdentical(IXLWorksheet worksheet, int columnIndex1, int columnIndex2,
        int? startRow = null, int? endRow = null)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null)
            return; // No data in worksheet, columns are considered identical

        int firstRow = startRow ?? usedRange.FirstRow().RowNumber();
        int lastRow = endRow ?? usedRange.LastRow().RowNumber();

        for (int row = firstRow; row <= lastRow; row++)
        {
            var cell1 = worksheet.Cell(row, columnIndex1);
            var cell2 = worksheet.Cell(row, columnIndex2);

            Assert.Equal(cell1.GetString(), cell2.GetString());

            // Also verify formulas if present
            if (!string.IsNullOrEmpty(cell1.FormulaA1))
                Assert.Equal(cell1.FormulaA1, cell2.FormulaA1);
            else if (!string.IsNullOrEmpty(cell2.FormulaA1))
                Assert.Fail($"Column {columnIndex1} has no formula at row {row}, but column {columnIndex2} has formula: {cell2.FormulaA1}");
        }
    }

    /// <summary>
    /// Verifies that two rows in different worksheets have identical values
    /// </summary>
    /// <param name="worksheet1">First worksheet</param>
    /// <param name="rowIndex1">Row index in first worksheet</param>
    /// <param name="worksheet2">Second worksheet</param>
    /// <param name="rowIndex2">Row index in second worksheet</param>
    /// <param name="startColumn">Optional: Starting column index (default: 1)</param>
    /// <param name="endColumn">Optional: Ending column index (default: maximum used columns across both sheets)</param>
    public static void VerifyRowsAreIdentical(IXLWorksheet worksheet1, int rowIndex1,
        IXLWorksheet worksheet2, int rowIndex2, int? startColumn = null, int? endColumn = null)
    {
        var usedRange1 = worksheet1.RangeUsed();
        var usedRange2 = worksheet2.RangeUsed();

        if (usedRange1 == null && usedRange2 == null)
            return; // No data in either worksheet, rows are considered identical

        int firstCol = startColumn ?? Math.Min(
            usedRange1?.FirstColumn().ColumnNumber() ?? 1,
            usedRange2?.FirstColumn().ColumnNumber() ?? 1);
        int lastCol = endColumn ?? Math.Max(
            usedRange1?.LastColumn().ColumnNumber() ?? 1,
            usedRange2?.LastColumn().ColumnNumber() ?? 1);

        for (int col = firstCol; col <= lastCol; col++)
        {
            var cell1 = worksheet1.Cell(rowIndex1, col);
            var cell2 = worksheet2.Cell(rowIndex2, col);

            Assert.Equal(cell1.GetString(), cell2.GetString());

            // Also verify formulas if present
            if (!string.IsNullOrEmpty(cell1.FormulaA1))
                Assert.Equal(cell1.FormulaA1, cell2.FormulaA1);
            else if (!string.IsNullOrEmpty(cell2.FormulaA1))
                Assert.Fail($"Worksheet '{worksheet1.Name}' row {rowIndex1} has no formula at column {col}, but worksheet '{worksheet2.Name}' row {rowIndex2} has formula: {cell2.FormulaA1}");
        }
    }

    /// <summary>
    /// Verifies that two columns in different worksheets have identical values
    /// </summary>
    /// <param name="worksheet1">First worksheet</param>
    /// <param name="columnIndex1">Column index in first worksheet</param>
    /// <param name="worksheet2">Second worksheet</param>
    /// <param name="columnIndex2">Column index in second worksheet</param>
    /// <param name="startRow">Optional: Starting row index (default: 1)</param>
    /// <param name="endRow">Optional: Ending row index (default: maximum used rows across both sheets)</param>
    public static void VerifyColumnsAreIdentical(IXLWorksheet worksheet1, int columnIndex1,
        IXLWorksheet worksheet2, int columnIndex2, int? startRow = null, int? endRow = null)
    {
        var usedRange1 = worksheet1.RangeUsed();
        var usedRange2 = worksheet2.RangeUsed();

        if (usedRange1 == null && usedRange2 == null)
            return; // No data in either worksheet, columns are considered identical

        int firstRow = startRow ?? Math.Min(
            usedRange1?.FirstRow().RowNumber() ?? 1,
            usedRange2?.FirstRow().RowNumber() ?? 1);
        int lastRow = endRow ?? Math.Max(
            usedRange1?.LastRow().RowNumber() ?? 1,
            usedRange2?.LastRow().RowNumber() ?? 1);

        for (int row = firstRow; row <= lastRow; row++)
        {
            var cell1 = worksheet1.Cell(row, columnIndex1);
            var cell2 = worksheet2.Cell(row, columnIndex2);

            Assert.Equal(cell1.GetString(), cell2.GetString());

            // Also verify formulas if present
            if (!string.IsNullOrEmpty(cell1.FormulaA1))
                Assert.Equal(cell1.FormulaA1, cell2.FormulaA1);
            else if (!string.IsNullOrEmpty(cell2.FormulaA1))
                Assert.Fail($"Worksheet '{worksheet1.Name}' column {columnIndex1} has no formula at row {row}, but worksheet '{worksheet2.Name}' column {columnIndex2} has formula: {cell2.FormulaA1}");
        }
    }

    public static void VerifyColumnMoved(IXLWorksheet worksheet, int sourceColumnIndex, int destinationColumnIndex, int? startRow = null, int? endRow = null)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null)
            return; // No data in worksheet, columns are considered identical

        int firstRow = startRow ?? usedRange.FirstRow().RowNumber();
        int lastRow = endRow ?? usedRange.LastRow().RowNumber();

        for (int row = firstRow; row <= lastRow; row++)
        {
            var sourceCell = worksheet.Cell(row, sourceColumnIndex);
            var destinationCell = worksheet.Cell(row, destinationColumnIndex);

            Assert.Equal("", sourceCell.GetString());
            Assert.NotEqual("", destinationCell.GetString());
        }
    }

    public static void VerifyRowMoved(IXLWorksheet worksheet, int sourceRowIndex, int destinationRowIndex, int? startColumn = null, int? endColumn = null)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null)
            return; // No data in worksheet, columns are considered identical

        int firstColumn = startColumn ?? usedRange.FirstColumn().ColumnNumber();
        int lastColumn = endColumn ?? usedRange.FirstColumn().ColumnNumber();

        for (int column = firstColumn; column <= lastColumn; column++)
        {
            var sourceCell = worksheet.Cell(sourceRowIndex, column);
            var destinationCell = worksheet.Cell(destinationRowIndex, column);

            Assert.Equal("", sourceCell.GetString());
            Assert.NotEqual("", destinationCell.GetString());
        }
    }


    private static void VerifyWorksheetsAreIdentical(IXLWorksheet inputSheet, IXLWorksheet outputSheet)
    {
        var inputUsedRange = inputSheet.RangeUsed();
        var outputUsedRange = outputSheet.RangeUsed();
        if (inputUsedRange != null && outputUsedRange != null)
        {
            Assert.Equal(inputUsedRange.RangeAddress.ToString(),
                        outputUsedRange.RangeAddress.ToString());
            foreach (var inputCell in inputUsedRange.Cells())
            {
                var outputCell = outputSheet.Cell(inputCell.Address);
                Assert.Equal(inputCell.GetString(), outputCell.GetString());
                if (!string.IsNullOrEmpty(inputCell.FormulaA1))
                    Assert.Equal(inputCell.FormulaA1, outputCell.FormulaA1);
            }
        }
        else
        {
            Assert.Equal(inputUsedRange, outputUsedRange);
        }
    }
}