namespace XLSXPipeline.Tests.PipelineTests.Worksheet.SetPageOrientation;

[Collection("FileAccess")]
public class SetPageOrientationTest : SetPageOrientationTestBase
{
    [Theory]
    [InlineData("Set Page Orientation")]
    public async Task SetPageOrientation_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetPageOrientation(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
