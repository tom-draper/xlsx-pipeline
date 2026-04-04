namespace XLSXPipeline.Tests.PipelineTests.Worksheet.SetPrintArea;

[Collection("FileAccess")]
public class SetPrintAreaTest : SetPrintAreaTestBase
{
    [Theory]
    [InlineData("Set Print Area")]
    public async Task SetPrintArea_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetPrintArea(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
