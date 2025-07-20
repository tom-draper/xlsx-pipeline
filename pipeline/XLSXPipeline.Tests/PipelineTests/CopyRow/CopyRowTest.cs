namespace XLSXPipeline.Tests.PipelineTests.CopyRow;

[Collection("FileAccess")]
public class CopyRowTest : CopyRowTestBase
{
    [Theory]
    [InlineData("Copy Row")]
    public async Task CopyRow_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            var result = await ExecutePipelineTestAsync(pipelineName);
            Assert.True(result.Success, $"Pipeline '{pipelineName}' should succeed. Error: {result.ErrorMessage}");
            VerifyCopyRow(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}