using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.DeleteHiddenSheets;

public abstract class DeleteHiddenSheetsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<DeleteHiddenSheetsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "DeleteHiddenSheets"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithHiddenSheet(inputPath);
            AddTempFile(inputPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    private static void CreateTestFileWithHiddenSheet(string path)
    {
        using var workbook = new XLWorkbook();
        var sheet1 = workbook.Worksheets.Add("Sheet1");
        sheet1.Cell("A1").Value = "Visible Data";
        var sheet2 = workbook.Worksheets.Add("Sheet2");
        sheet2.Cell("A1").Value = "Hidden Data";
        sheet2.Visibility = XLWorksheetVisibility.Hidden;
        workbook.SaveAs(path);
    }

    protected void VerifyHiddenSheetsDeleted(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        Assert.False(workbook.Worksheets.Contains("Sheet2"), "Expected hidden sheet 'Sheet2' to be deleted");
        Assert.True(workbook.Worksheets.Contains("Sheet1"), "Expected visible sheet 'Sheet1' to remain");
    }
}
