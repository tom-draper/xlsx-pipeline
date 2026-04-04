namespace XLSXPipeline.Tests.PipelineTests.Worksheet.SetPageMargins;

[Collection("FileAccess")]
public class SetPageMarginsTest : SetPageMarginsTestBase
{
    [Theory]
    [InlineData("Set Page Margins")]
    public async Task SetPageMargins_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetPageMargins(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
