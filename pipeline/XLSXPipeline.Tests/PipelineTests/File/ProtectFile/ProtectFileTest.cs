namespace XLSXPipeline.Tests.PipelineTests.File.ProtectFile;

[Collection("FileAccess")]
public class ProtectFileTest : ProtectFileTestBase
{
    [Theory]
    [InlineData("Protect File")]
    public async Task ProtectFile_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileIsProtected(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
