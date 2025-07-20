namespace XLSXPipeline.Tests.PipelineTests.CopyColumn;

[Collection("FileAccess")]
public class CopyColumnTest : CopyColumnTestBase
{
    [Theory]
    [InlineData("Copy Column")]
    public async Task CopyColumn_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            var result = await ExecutePipelineTestAsync(pipelineName);
            Assert.True(result.Success, $"Pipeline '{pipelineName}' should succeed. Error: {result.ErrorMessage}");
            VerifyCopyColumn(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}