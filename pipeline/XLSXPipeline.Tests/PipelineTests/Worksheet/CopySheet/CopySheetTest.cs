namespace XLSXPipeline.Tests.PipelineTests.Worksheet.CopySheet;

[Collection("FileAccess")]
public class CopySheetTest : CopySheetTestBase
{
    [Theory]
    [InlineData("Copy Sheet")]
    public async Task CopySheet_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySheetCopied("Sheet1Copy", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
