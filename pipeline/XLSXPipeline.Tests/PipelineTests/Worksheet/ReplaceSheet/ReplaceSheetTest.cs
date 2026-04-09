namespace XLSXPipeline.Tests.PipelineTests.Worksheet.ReplaceSheet;

[Collection("FileAccess")]
public class ReplaceSheetTest : ReplaceSheetTestBase
{
    [Theory]
    [InlineData("Replace Sheet")]
    public async Task ReplaceSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetReplaced(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
