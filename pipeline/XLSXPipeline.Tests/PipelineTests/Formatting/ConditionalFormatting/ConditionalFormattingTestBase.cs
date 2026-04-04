using ClosedXML.Excel;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Formatting.ConditionalFormatting;

public abstract class ConditionalFormattingTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ConditionalFormattingAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Formatting", "ConditionalFormatting"), defaultPipelineName)
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

    protected void VerifyConditionalFormatting(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        Assert.True(worksheet.ConditionalFormats.Count() > 0, "Worksheet should have at least one conditional format.");
    }
}
