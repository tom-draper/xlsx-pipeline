namespace XLSXPipeline.Tests.PipelineTests.File.MoveFile;

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
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileIntegrity(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}