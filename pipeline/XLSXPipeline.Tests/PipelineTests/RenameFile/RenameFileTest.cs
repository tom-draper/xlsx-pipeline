using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.RenameFile;

[Collection("FileAccess")]
public class RenameFileTest : RenameFileTestBase
{
    [Fact]
    public async Task RenameFilePipeline()
    {
        string pipelineName = "Rename File";
        var success = await ExecuteRenameFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Rename file operation should succeed");

        // Re-execute for verification
        await VerifyPipelineOutput(pipelineName);
    }

    [Fact]
    public async Task RenameFileNoExtensionPipeline()
    {
        string pipelineName = "Rename File No Extension";
        var success = await ExecuteRenameFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Rename file operation should succeed");

        // Re-execute for verification
        await VerifyPipelineOutput(pipelineName);
    }


    private async Task VerifyPipelineOutput(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;

        string inputPath = GetInputPath(pipelineName);
        string outputPath = GetOutputPath(pipelineName);
        var pipeline = GetPipeline(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);
            AddTempFile(outputPath);

            var pipelineExecutor = await GetPipelineExecutorAsync();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);

            VerifyRenamedFile(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}