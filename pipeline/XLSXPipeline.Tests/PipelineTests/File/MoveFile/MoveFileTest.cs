namespace XLSXPipeline.Tests.PipelineTests.File.MoveFile;

[Collection("FileAccess")]
public class MoveFileTest : MoveFileTestBase
{
    [Theory]
    [InlineData("Move File")]
    [InlineData("Move File No Extension")]
    [InlineData("Move File Nested")]
    [InlineData("Move File with Date")]
    [InlineData("Move File with DateTime")]
    [InlineData("Move File with DateTime Path")]
    [InlineData("Move File Twice")]
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