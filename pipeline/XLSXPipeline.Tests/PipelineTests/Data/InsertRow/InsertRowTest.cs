namespace XLSXPipeline.Tests.PipelineTests.Data.InsertRow;

[Collection("FileAccess")]
public class InsertRowTest : InsertRowTestBase
{
    [Theory]
    [InlineData("Insert Row")]
    public async Task InsertRow_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyRowInserted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
