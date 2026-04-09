namespace XLSXPipeline.Tests.PipelineTests.Worksheet.UnprotectSheet;

[Collection("FileAccess")]
public class UnprotectSheetTest : UnprotectSheetTestBase
{
    [Theory]
    [InlineData("Unprotect Sheet")]
    public async Task UnprotectSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetUnprotected("Sheet1", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
