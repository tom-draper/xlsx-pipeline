using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.CopyFile;

[Collection("FileAccess")]
public class CopyFileTest : CopyFileTestBase
{
    [Fact]
    public async Task CopyFilePipeline()
    {
        string pipelineName = "Copy File";
        var success = await ExecuteCopyFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Copy file operation should succeed");

        // Re-execute for verification
        await VerifyPipelineOutput(pipelineName);
    }

    [Fact]
    public async Task CopyFileNoExtensionPipeline()
    {
        string pipelineName = "Copy File No Extension";
        var success = await ExecuteCopyFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Copy file operation should succeed");

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

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);

            VerifyFileIntegrity(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
