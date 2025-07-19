namespace XLSXPipeline.Tests.Test1;

public class Test1
{
    public Test1()
    {
        TestSetup.CreateTestExcelFile();
    }

    public static void CreateTestExcelFile()
    {
        using (var workbook = new XLWorkbook())
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "Name";
        worksheet.Cell("B1").Value = "Age";
        worksheet.Cell("A2").Value = "John";
        worksheet.Cell("B2").Value = 25;
        workbook.SaveAs(".\\input\\TestSheet.xlsx");
    }

    [Fact]
    public void Test1()
    {
        var worker = XLSXPipeline.Worker.CreateWorker();

    }
}

