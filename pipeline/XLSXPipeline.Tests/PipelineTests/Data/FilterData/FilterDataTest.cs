namespace XLSXPipeline.Tests.PipelineTests.Data.FilterData;

[Collection("FileAccess")]
public class FilterDataTest : FilterDataTestBase
{
    [Theory]
    [InlineData("Filter Data")]
    public async Task FilterData_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFilterApplied(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
