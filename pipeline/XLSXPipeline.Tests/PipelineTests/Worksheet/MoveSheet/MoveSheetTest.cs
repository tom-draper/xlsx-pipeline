namespace XLSXPipeline.Tests.PipelineTests.Worksheet.MoveSheet;

[Collection("FileAccess")]
public class MoveSheetTest : MoveSheetTestBase
{
    [Theory]
    [InlineData("Move Sheet")]
    public async Task MoveSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetMoved("Sheet1", 2, pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
