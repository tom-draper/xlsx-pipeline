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
        {
            worksheet.Cell(kvp.Key).Value = (XLCellValue)kvp.Value;
        }

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
                {
                    Assert.Equal(inputCell.FormulaA1, outputCell.FormulaA1);
                }
            }
        }
        else
        {
            Assert.Equal(inputUsedRange, outputUsedRange);
        }
    }
}