namespace XLSXPipeline.Tests.PipelineTests.Data.DeleteColumn;

[Collection("FileAccess")]
public class DeleteColumnTest : DeleteColumnTestBase
{
    [Theory]
    [InlineData("Delete Column")]
    public async Task DeleteColumn_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyColumnDeleted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
