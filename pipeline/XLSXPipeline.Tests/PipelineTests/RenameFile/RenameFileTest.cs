namespace XLSXPipeline.Tests.PipelineTests.RenameFile;

[Collection("FileAccess")]
public class RenameFileTest : RenameFileTestBase
{
    [Theory]
    [InlineData("Rename File")]
    [InlineData("Rename File No Extension")]
    [InlineData("Rename File Output Path")]
    public async Task RenameFile_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            var result = await ExecutePipelineTestAsync(pipelineName);
            Assert.True(result.Success, $"Pipeline '{pipelineName}' should succeed. Error: {result.ErrorMessage}");
            VerifyRenamedFile(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}