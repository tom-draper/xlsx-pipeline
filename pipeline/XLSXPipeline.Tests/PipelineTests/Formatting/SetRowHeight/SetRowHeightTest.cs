namespace XLSXPipeline.Tests.PipelineTests.Formatting.SetRowHeight;

[Collection("FileAccess")]
public class SetRowHeightTest : SetRowHeightTestBase
{
    [Theory]
    [InlineData("Set Row Height")]
    public async Task SetRowHeight_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetRowHeight(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
