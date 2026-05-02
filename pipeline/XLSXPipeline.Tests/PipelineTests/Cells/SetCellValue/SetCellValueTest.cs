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

    [Fact]
    public async Task SetCellValue_WithPlaceholders_ShouldResolvePlaceholders()
    {
        string pipelineName = "Set Cell Value Placeholders";
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            
            var now = DateTime.Now;
            string expectedValue = $"Year: {now.Year:D4}, Month: {now.Month:D2}";
            
            VerifyCellValue("A1", expectedValue, pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
