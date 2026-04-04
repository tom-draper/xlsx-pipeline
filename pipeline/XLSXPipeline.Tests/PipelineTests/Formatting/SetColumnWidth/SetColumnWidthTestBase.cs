using ClosedXML.Excel;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Formatting.SetColumnWidth;

public abstract class SetColumnWidthTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<SetColumnWidthAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Formatting", "SetColumnWidth"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
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

    protected void VerifySetColumnWidth(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        Assert.Equal(25.0, worksheet.Column(1).Width);
    }
}
