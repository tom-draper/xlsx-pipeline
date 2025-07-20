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
            await ExecutePipelineTestAsync(pipelineName);
            VerifyRenamedFile(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}