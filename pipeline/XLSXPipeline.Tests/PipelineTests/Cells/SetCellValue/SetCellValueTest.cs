namespace XLSXPipeline.Tests.PipelineTests.Cells.SetCellValue;

[Collection("FileAccess")]
public class SetCellValueTest : SetCellValueTestBase
{
    [Theory]
    [InlineData("Set Cell Value")]
    public async Task SetCellValue_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyCellValue("C1", "Hello World", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
