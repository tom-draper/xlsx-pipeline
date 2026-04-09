namespace XLSXPipeline.Tests.PipelineTests.File.UnprotectFile;

[Collection("FileAccess")]
public class UnprotectFileTest : UnprotectFileTestBase
{
    [Theory]
    [InlineData("Unprotect File")]
    public async Task UnprotectFile_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileIsUnprotected(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
