namespace XLSXPipeline.Tests.PipelineTests.Formatting.ConditionalFormatting;

[Collection("FileAccess")]
public class ConditionalFormattingTest : ConditionalFormattingTestBase
{
    [Theory]
    [InlineData("Conditional Formatting")]
    public async Task ConditionalFormatting_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyConditionalFormatting(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
