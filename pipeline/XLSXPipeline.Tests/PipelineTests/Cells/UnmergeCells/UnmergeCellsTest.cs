namespace XLSXPipeline.Tests.PipelineTests.Cells.UnmergeCells;

[Collection("FileAccess")]
public class UnmergeCellsTest : UnmergeCellsTestBase
{
    [Theory]
    [InlineData("Unmerge Cells")]
    public async Task UnmergeCells_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyUnmergeCells(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
