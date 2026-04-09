namespace XLSXPipeline.Tests.PipelineTests.File.DeleteFile;

[Collection("FileAccess")]
public class DeleteFileTest : DeleteFileTestBase
{
    [Theory]
    [InlineData("Delete File")]
    public async Task DeleteFile_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        var inputPath = GetInputPath(pipelineName);
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileDeleted(inputPath);
        }
        finally
        {
            await CleanupTempFilesAsync();
            // Ensure cleanup in case test failed before deletion
            if (System.IO.File.Exists(inputPath))
                System.IO.File.Delete(inputPath);
        }
    }
}
