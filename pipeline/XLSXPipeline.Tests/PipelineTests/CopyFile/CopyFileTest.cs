using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.CopyFile;

public class CopyFileTest : CopyFileTestBase
{
    [Fact]
    public async Task CopyFile_CopiesFileToDestination()
    {
        // Execute the test
        var success = await ExecuteCopyFileTestAsync();

        // Verify results
        Assert.True(success, "File copy operation should succeed");

        // Re-create files for verification (since cleanup happened in ExecuteCopyFileTestAsync)
        ExcelTestHelpers.CreateTestFile(InputPath);
        AddTempFile(InputPath);
        AddTempFile(OutputPath);

        var pipelineExecutor = await GetPipelineExecutorAsync();
        await pipelineExecutor.ExecutePipelineAsync(Pipeline, InputPath);

        VerifyFileIntegrity();
    }
}