namespace XLSXPipeline.Tests.PipelineTests.File.ImportCSV;

[Collection("FileAccess")]
public class ImportCSVTest : ImportCSVTestBase
{
    [Theory]
    [InlineData("Import CSV")]
    public async Task ImportCSV_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyImportCSV(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
