namespace XLSXPipeline.Tests.PipelineTests.Cells.FindAndReplace;

[Collection("FileAccess")]
public class FindAndReplaceTest : FindAndReplaceTestBase
{
    [Theory]
    [InlineData("Find And Replace")]
    public async Task FindAndReplace_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFindAndReplace(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
