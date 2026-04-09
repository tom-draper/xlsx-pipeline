using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.SortData;

public abstract class SortDataTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<SortDataAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "SortData"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateUnsortedTestFile(inputPath);
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

    private static void CreateUnsortedTestFile(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = 3;
        worksheet.Cell("A2").Value = 1;
        worksheet.Cell("A3").Value = 2;
        workbook.SaveAs(path);
    }

    protected void VerifyDataSorted(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        Assert.Equal("1", worksheet.Cell("A1").GetString());
        Assert.Equal("2", worksheet.Cell("A2").GetString());
        Assert.Equal("3", worksheet.Cell("A3").GetString());
    }
}
