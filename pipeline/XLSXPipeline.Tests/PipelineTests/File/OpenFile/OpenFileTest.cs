namespace XLSXPipeline.Tests.PipelineTests.File.OpenFile;

[Collection("FileAccess")]
public class OpenFileTest : OpenFileTestBase
{
    [Theory]
    [InlineData("Open File")]
    public async Task OpenFile_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            // OpenFile launches the OS process; we just verify no exception is thrown
            // and the file still exists after execution
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileStillExists(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
