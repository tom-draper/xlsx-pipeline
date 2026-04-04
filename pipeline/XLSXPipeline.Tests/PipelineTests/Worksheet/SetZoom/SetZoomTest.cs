namespace XLSXPipeline.Tests.PipelineTests.Worksheet.SetZoom;

[Collection("FileAccess")]
public class SetZoomTest : SetZoomTestBase
{
    [Theory]
    [InlineData("Set Zoom")]
    public async Task SetZoom_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetZoom(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
