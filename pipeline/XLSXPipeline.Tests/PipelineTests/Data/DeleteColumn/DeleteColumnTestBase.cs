using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.DeleteColumn;

public abstract class DeleteColumnTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<DeleteColumnAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "DeleteColumn"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithColumns(inputPath);
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

    private static void CreateTestFileWithColumns(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "ColA";
        worksheet.Cell("B1").Value = "ColB";
        worksheet.Cell("C1").Value = "ColC";
        workbook.SaveAs(path);
    }

    protected void VerifyColumnDeleted(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // After deleting column B, original C ("ColC") should shift to B
        Assert.Equal("ColA", worksheet.Cell("A1").GetString());
        Assert.Equal("ColC", worksheet.Cell("B1").GetString());
    }
}
