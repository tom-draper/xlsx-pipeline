namespace XLSXPipeline.Tests.PipelineTests.Time.Wait;

[Collection("FileAccess")]
public class WaitTest : WaitTestBase
{
    [Theory]
    [InlineData("Wait")]
    public async Task Wait_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyWaitCompleted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
