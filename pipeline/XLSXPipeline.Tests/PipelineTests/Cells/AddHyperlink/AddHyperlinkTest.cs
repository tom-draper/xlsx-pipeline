namespace XLSXPipeline.Tests.PipelineTests.Cells.AddHyperlink;

[Collection("FileAccess")]
public class AddHyperlinkTest : AddHyperlinkTestBase
{
    [Theory]
    [InlineData("Add Hyperlink")]
    public async Task AddHyperlink_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyAddHyperlink(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
