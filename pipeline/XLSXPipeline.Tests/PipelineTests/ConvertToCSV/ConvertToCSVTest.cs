using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.ConvertToCSV;

[Collection("FileAccess")]
public class ConvertToCSVTest : ConvertToCSVTestBase
{
    [Fact]
    public async Task ConvertToCSVPipeline()
    {
        string pipelineName = "Convert To CSV";
        var success = await ExecuteConvertToCSVTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Convert to CSV operation should succeed");

        // Re-execute for verification
        await VerifyPipelineOutput(pipelineName);
    }

    [Fact]
    public async Task ConvertToCSVNoExtensionPipeline()
    {
        string pipelineName = "Convert To CSV No Extension";
        var success = await ExecuteConvertToCSVTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Convert to CSV operation should succeed");

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