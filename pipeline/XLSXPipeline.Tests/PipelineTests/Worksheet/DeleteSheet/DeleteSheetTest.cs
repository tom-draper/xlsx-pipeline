namespace XLSXPipeline.Tests.PipelineTests.Worksheet.DeleteSheet;

[Collection("FileAccess")]
public class DeleteSheetTest : DeleteSheetTestBase
{
    [Theory]
    [InlineData("Delete Sheet")]
    public async Task DeleteSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetDeleted("Sheet2", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
