namespace XLSXPipeline.Tests.PipelineTests.MoveFile;

[Collection("FileAccess")]
public class MoveFileTest : MoveFileTestBase
{
    [Theory]
    [InlineData("Move File")]
    [InlineData("Move File No Extension")]
    [InlineData("Move File Nested")]
    public async Task MoveFile_SpecificPipeline_ShouldSucceed(string pipelineName)
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