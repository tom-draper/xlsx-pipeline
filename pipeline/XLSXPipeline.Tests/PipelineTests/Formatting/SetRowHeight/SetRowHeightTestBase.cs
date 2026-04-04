using ClosedXML.Excel;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Formatting.SetRowHeight;

public abstract class SetRowHeightTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<SetRowHeightAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Formatting", "SetRowHeight"), defaultPipelineName)
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

    protected void VerifySetRowHeight(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        Assert.Equal(30.0, worksheet.Row(1).Height);
    }
}
