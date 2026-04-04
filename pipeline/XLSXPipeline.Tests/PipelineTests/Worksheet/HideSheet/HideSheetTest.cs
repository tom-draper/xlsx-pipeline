namespace XLSXPipeline.Tests.PipelineTests.Worksheet.HideSheet;

[Collection("FileAccess")]
public class HideSheetTest : HideSheetTestBase
{
    [Theory]
    [InlineData("Hide Sheet")]
    public async Task HideSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyHideSheet(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
