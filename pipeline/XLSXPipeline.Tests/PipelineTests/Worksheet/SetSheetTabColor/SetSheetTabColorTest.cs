namespace XLSXPipeline.Tests.PipelineTests.Worksheet.SetSheetTabColor;

[Collection("FileAccess")]
public class SetSheetTabColorTest : SetSheetTabColorTestBase
{
    [Theory]
    [InlineData("Set Sheet Tab Color")]
    public async Task SetSheetTabColor_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetSheetTabColor(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
