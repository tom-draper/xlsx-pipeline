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
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileIntegrity(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
