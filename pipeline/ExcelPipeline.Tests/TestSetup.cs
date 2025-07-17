using ClosedXML.Excel;

namespace ExcelPipeline.Tests
{
    public class TestSetup
    {
        public static void CreateTestExcelFile()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell("A1").Value = "Name";
                worksheet.Cell("B1").Value = "Age";
                worksheet.Cell("A2").Value = "John";
                worksheet.Cell("B2").Value = 25;
                workbook.SaveAs(".\\TestSheet.xlsx");
            }
        }
    }
}
