namespace XLSXPipeline.Tests.PipelineTests.Worksheet.UnhideSheet;

[Collection("FileAccess")]
public class UnhideSheetTest : UnhideSheetTestBase
{
    [Theory]
    [InlineData("Unhide Sheet")]
    public async Task UnhideSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyUnhideSheet(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
