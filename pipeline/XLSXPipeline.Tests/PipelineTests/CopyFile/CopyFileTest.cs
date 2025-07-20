namespace XLSXPipeline.Tests.PipelineTests.CopyFile;

[Collection("FileAccess")]
public class CopyFileTest : CopyFileTestBase
{
    [Theory]
    [InlineData("Copy File")]
    [InlineData("Copy File No Extension")]
    [InlineData("Copy File Nested")]
    public async Task CopyFile_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            var result = await ExecutePipelineTestAsync(pipelineName);
            Assert.True(result.Success, $"Pipeline '{pipelineName}' should succeed. Error: {result.ErrorMessage}");
            VerifyFileIntegrity(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
