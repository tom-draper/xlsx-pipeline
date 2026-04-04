namespace XLSXPipeline.Tests.PipelineTests.Worksheet.AddNamedRange;

[Collection("FileAccess")]
public class AddNamedRangeTest : AddNamedRangeTestBase
{
    [Theory]
    [InlineData("Add Named Range")]
    public async Task AddNamedRange_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyAddNamedRange(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
