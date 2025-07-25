namespace XLSXPipeline.Tests.PipelineTests.File.ExportToCSV;

[Collection("FileAccess")]
public class ExportToCSVTest : ExportToCSVTestBase
{
    [Theory]
    [InlineData("Export To CSV")]
    [InlineData("Export To CSV No Extension")]
    [InlineData("Export To CSV Nested")]
    public async Task ExportToCSV_SpecificPipeline_ShouldSucceed(string pipelineName)
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