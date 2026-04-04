namespace XLSXPipeline.Tests.PipelineTests.Data.DeduplicateRows;

[Collection("FileAccess")]
public class DeduplicateRowsTest : DeduplicateRowsTestBase
{
    [Theory]
    [InlineData("Deduplicate Rows")]
    public async Task DeduplicateRows_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyDeduplicateRows(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
