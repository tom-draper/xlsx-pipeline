namespace XLSXPipeline.Tests.PipelineTests.Worksheet.AddSheet;

[Collection("FileAccess")]
public class AddSheetTest : AddSheetTestBase
{
    [Theory]
    [InlineData("Add Sheet")]
    public async Task AddSheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetAdded("NewSheet", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
