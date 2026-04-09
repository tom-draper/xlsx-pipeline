using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.FilterData;

public abstract class FilterDataTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<FilterDataAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "FilterData"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithData(inputPath);
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

    private static void CreateTestFileWithData(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "Alpha";
        worksheet.Cell("B1").Value = 10;
        worksheet.Cell("A2").Value = "Beta";
        worksheet.Cell("B2").Value = 20;
        worksheet.Cell("A3").Value = "Gamma";
        worksheet.Cell("B3").Value = 30;
        workbook.SaveAs(path);
    }

    protected void VerifyFilterApplied(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // Auto filter should be set on the range
        Assert.NotNull(worksheet.AutoFilter);
    }
}
