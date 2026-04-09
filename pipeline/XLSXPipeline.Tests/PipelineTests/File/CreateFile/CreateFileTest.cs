namespace XLSXPipeline.Tests.PipelineTests.File.CreateFile;

[Collection("FileAccess")]
public class CreateFileTest : CreateFileTestBase
{
    [Theory]
    [InlineData("Create File")]
    public async Task CreateFile_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileCreated(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
