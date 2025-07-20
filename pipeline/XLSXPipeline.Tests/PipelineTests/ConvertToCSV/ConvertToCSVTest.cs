namespace XLSXPipeline.Tests.PipelineTests.ConvertToCSV;

[Collection("FileAccess")]
public class ConvertToCSVTest : ConvertToCSVTestBase
{
    [Theory]
    [InlineData("Convert To CSV")]
    [InlineData("Convert To CSV No Extension")]
    [InlineData("Convert To CSV Nested")]
    public async Task ConvertToCSV_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFileIntegrity(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}