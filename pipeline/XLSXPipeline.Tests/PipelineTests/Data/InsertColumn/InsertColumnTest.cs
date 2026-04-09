namespace XLSXPipeline.Tests.PipelineTests.Data.InsertColumn;

[Collection("FileAccess")]
public class InsertColumnTest : InsertColumnTestBase
{
    [Theory]
    [InlineData("Insert Column")]
    public async Task InsertColumn_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyColumnInserted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
