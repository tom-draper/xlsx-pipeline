namespace XLSXPipeline.Tests.PipelineTests.File.ExportToPDF;

[Collection("FileAccess")]
public class ExportToPDFTest : ExportToPDFTestBase
{
    [Theory]
    [InlineData("Export To PDF")]
    [InlineData("Export To PDF No Extension")]
    [InlineData("Export To PDF Nested")]
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