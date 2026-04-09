namespace XLSXPipeline.Tests.PipelineTests.Formatting.FormatCells;

[Collection("FileAccess")]
public class FormatCellsTest : FormatCellsTestBase
{
    [Theory]
    [InlineData("Format Cells")]
    public async Task FormatCells_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyCellsFormatted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
