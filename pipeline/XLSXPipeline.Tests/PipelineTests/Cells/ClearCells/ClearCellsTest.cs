namespace XLSXPipeline.Tests.PipelineTests.Cells.ClearCells;

[Collection("FileAccess")]
public class ClearCellsTest : ClearCellsTestBase
{
    [Theory]
    [InlineData("Clear Cells")]
    public async Task ClearCells_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyClearCells(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
