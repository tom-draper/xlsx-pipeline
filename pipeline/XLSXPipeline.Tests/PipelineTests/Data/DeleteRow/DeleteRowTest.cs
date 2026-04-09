namespace XLSXPipeline.Tests.PipelineTests.Data.DeleteRow;

[Collection("FileAccess")]
public class DeleteRowTest : DeleteRowTestBase
{
    [Theory]
    [InlineData("Delete Row")]
    public async Task DeleteRow_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyRowDeleted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
