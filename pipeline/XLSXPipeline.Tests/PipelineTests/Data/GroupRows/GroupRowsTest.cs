namespace XLSXPipeline.Tests.PipelineTests.Data.GroupRows;

[Collection("FileAccess")]
public class GroupRowsTest : GroupRowsTestBase
{
    [Theory]
    [InlineData("Group Rows")]
    public async Task GroupRows_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyGroupRows(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
