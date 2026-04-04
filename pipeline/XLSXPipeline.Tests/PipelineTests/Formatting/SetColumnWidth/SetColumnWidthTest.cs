namespace XLSXPipeline.Tests.PipelineTests.Formatting.SetColumnWidth;

[Collection("FileAccess")]
public class SetColumnWidthTest : SetColumnWidthTestBase
{
    [Theory]
    [InlineData("Set Column Width")]
    public async Task SetColumnWidth_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySetColumnWidth(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
