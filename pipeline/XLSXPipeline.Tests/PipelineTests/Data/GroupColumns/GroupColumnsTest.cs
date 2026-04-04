namespace XLSXPipeline.Tests.PipelineTests.Data.GroupColumns;

[Collection("FileAccess")]
public class GroupColumnsTest : GroupColumnsTestBase
{
    [Theory]
    [InlineData("Group Columns")]
    public async Task GroupColumns_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyGroupColumns(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
