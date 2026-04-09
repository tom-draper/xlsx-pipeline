namespace XLSXPipeline.Tests.PipelineTests.Worksheet.DeleteHiddenSheets;

[Collection("FileAccess")]
public class DeleteHiddenSheetsTest : DeleteHiddenSheetsTestBase
{
    [Theory]
    [InlineData("Delete Hidden Sheets")]
    public async Task DeleteHiddenSheets_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyHiddenSheetsDeleted(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
