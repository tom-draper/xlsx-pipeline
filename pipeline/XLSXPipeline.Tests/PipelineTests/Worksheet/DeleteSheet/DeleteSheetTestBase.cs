using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.DeleteSheet;

public abstract class DeleteSheetTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<DeleteSheetAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "DeleteSheet"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithTwoSheets(inputPath);
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

    private static void CreateTestFileWithTwoSheets(string path)
    {
        using var workbook = new XLWorkbook();
        var sheet1 = workbook.Worksheets.Add("Sheet1");
        sheet1.Cell("A1").Value = "Sheet1 Data";
        var sheet2 = workbook.Worksheets.Add("Sheet2");
        sheet2.Cell("A1").Value = "Sheet2 Data";
        workbook.SaveAs(path);
    }

    protected void VerifySheetDeleted(string deletedSheetName, string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        Assert.False(workbook.Worksheets.Contains(deletedSheetName), $"Expected sheet '{deletedSheetName}' to have been deleted");
    }
}
