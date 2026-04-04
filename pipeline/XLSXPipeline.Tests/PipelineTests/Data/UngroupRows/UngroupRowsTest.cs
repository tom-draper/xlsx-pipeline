namespace XLSXPipeline.Tests.PipelineTests.Data.UngroupRows;

[Collection("FileAccess")]
public class UngroupRowsTest : UngroupRowsTestBase
{
    [Theory]
    [InlineData("Ungroup Rows")]
    public async Task UngroupRows_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyUngroupRows(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
