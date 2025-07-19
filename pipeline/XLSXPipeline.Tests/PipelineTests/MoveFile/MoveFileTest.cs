using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.MoveFile;

[Collection("FileAccess")]
public class MoveFileTest : MoveFileTestBase
{
    [Fact]
    public async Task MoveFilePipeline()
    {
        string pipelineName = "Move File";
        var success = await ExecuteMoveFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Move copy operation should succeed");

        // Re-execute for verification
        await VerifyPipelineOutput(pipelineName);
    }

    [Fact]
    public async Task MoveFileNoExtensionPipeline()
    {
        string pipelineName = "Move File No Extension";
        var success = await ExecuteMoveFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Move copy operation should succeed");

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

            VerifyFileIntegrity(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}