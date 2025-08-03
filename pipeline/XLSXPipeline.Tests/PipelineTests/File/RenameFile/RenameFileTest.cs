namespace XLSXPipeline.Tests.PipelineTests.File.RenameFile;

[Collection("FileAccess")]
public class RenameFileTest : RenameFileTestBase
{
    [Theory]
    [InlineData("Rename File")]
    [InlineData("Rename File No Extension")]
    [InlineData("Rename File Output Path")]
    [InlineData("Rename File with Date")]
    [InlineData("Rename File with DateTime")]
    [InlineData("Rename File with DateTime Path")]
    [InlineData("Rename File Twice")]
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