namespace XLSXPipeline.Tests.PipelineTests.Data.MergeData;

[Collection("FileAccess")]
public class MergeDataTest : MergeDataTestBase
{
    [Theory]
    [InlineData("Merge Data")]
    public async Task MergeData_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyDataMerged(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
