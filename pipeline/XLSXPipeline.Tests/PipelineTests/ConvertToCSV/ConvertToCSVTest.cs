using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.ConvertToCSV;

public class ConvertToCSVTest : ConvertToCSVTestBase
{
    [Fact]
    public async Task ConvertToCSV_ConvertToCSV()
    {
        // Execute the test
        var success = await ExecuteConvertToCSVAsync();

        // Verify results
        Assert.True(success, "Convert to CSV operation should succeed");

        // Re-create files for verification (since cleanup happened in ExecuteCopyFileTestAsync)
        ExcelTestHelpers.CreateTestFile(InputPath);
        AddTempFile(InputPath);
        AddTempFile(OutputPath);

        var pipelineExecutor = await GetPipelineExecutorAsync();
        await pipelineExecutor.ExecutePipelineAsync(Pipeline, InputPath);

        VerifyFileIntegrity();
    }
}