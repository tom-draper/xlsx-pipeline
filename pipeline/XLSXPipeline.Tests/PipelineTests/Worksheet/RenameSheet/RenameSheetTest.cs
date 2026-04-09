namespace XLSXPipeline.Tests.PipelineTests.Worksheet.RenameSheet;

[Collection("FileAccess")]
public class RenameSheetTest : RenameSheetTestBase
{
    [Theory]
    [InlineData("Rename Sheet")]
    public async Task RenameSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetRenamed("Sheet1", "Renamed", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
