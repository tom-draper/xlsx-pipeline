namespace XLSXPipeline.Tests.PipelineTests.Data.SortData;

[Collection("FileAccess")]
public class SortDataTest : SortDataTestBase
{
    [Theory]
    [InlineData("Sort Data")]
    public async Task SortData_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyDataSorted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
