using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.RenameFile;

public class RenameFileTest : RenameFileTestBase
{
    [Fact]
    public async Task RenameFile_RenamesFileCorrectly()
    {
        // Execute the test
        var success = await ExecuteRenameFileTestAsync();

        // Verify results
        Assert.True(success, "File rename operation should succeed");

        // Re-create files for verification (since cleanup happened in ExecuteRenameFileTestAsync)
        ExcelTestHelpers.CreateTestFile(InputPath);
        AddTempFile(InputPath);
        AddTempFile(OutputPath);

        var pipelineExecutor = await GetPipelineExecutorAsync();
        await pipelineExecutor.ExecutePipelineAsync(Pipeline, InputPath);

        VerifyRenamedFile();
    }
}