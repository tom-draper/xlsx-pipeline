namespace XLSXPipeline.Tests.PipelineTests.Cells.MergeCells;

[Collection("FileAccess")]
public class MergeCellsTest : MergeCellsTestBase
{
    [Theory]
    [InlineData("Merge Cells")]
    public async Task MergeCells_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyMergeCells(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
