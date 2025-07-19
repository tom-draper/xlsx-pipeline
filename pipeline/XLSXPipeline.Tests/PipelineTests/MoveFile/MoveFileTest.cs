using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.MoveFile;

public class MoveFileTest : MoveFileTestBase
{
    [Fact]
    public async Task MoveFile_MovesFileToDestination()
    {
        // Execute the test
        var success = await ExecuteMoveFileTestAsync();

        // Verify results
        Assert.True(success, "File move operation should succeed");

        // Re-create files for verification (since cleanup happened in ExecuteMoveFileTestAsync)
        ExcelTestHelpers.CreateTestFile(InputPath);
        AddTempFile(InputPath);
        AddTempFile(OutputPath);

        var pipelineExecutor = await GetPipelineExecutorAsync();
        await pipelineExecutor.ExecutePipelineAsync(Pipeline, InputPath);

        VerifyMovedFile();
    }
}