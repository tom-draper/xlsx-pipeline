namespace XLSXPipeline.Tests.PipelineTests.Worksheet.SetHeaderFooter;

[Collection("FileAccess")]
public class SetHeaderFooterTest : SetHeaderFooterTestBase
{
    [Theory]
    [InlineData("Set Header Footer")]
    public async Task SetHeaderFooter_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetHeaderFooter(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
