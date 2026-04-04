namespace XLSXPipeline.Tests.PipelineTests.Worksheet.FreezePane;

[Collection("FileAccess")]
public class FreezePaneTest : FreezePaneTestBase
{
    [Theory]
    [InlineData("Freeze Pane")]
    public async Task FreezePane_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFreezePane(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
