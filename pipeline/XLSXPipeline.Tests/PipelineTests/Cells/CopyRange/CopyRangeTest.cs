namespace XLSXPipeline.Tests.PipelineTests.Cells.CopyRange;

[Collection("FileAccess")]
public class CopyRangeTest : CopyRangeTestBase
{
    [Theory]
    [InlineData("Copy Range")]
    public async Task CopyRange_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyCopyRange(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
