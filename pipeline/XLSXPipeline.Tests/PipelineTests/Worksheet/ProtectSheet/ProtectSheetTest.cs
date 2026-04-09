namespace XLSXPipeline.Tests.PipelineTests.Worksheet.ProtectSheet;

[Collection("FileAccess")]
public class ProtectSheetTest : ProtectSheetTestBase
{
    [Theory]
    [InlineData("Protect Sheet")]
    public async Task ProtectSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetProtected("Sheet1", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
