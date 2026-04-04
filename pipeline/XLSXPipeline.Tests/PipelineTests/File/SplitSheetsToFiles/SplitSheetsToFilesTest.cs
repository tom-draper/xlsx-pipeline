namespace XLSXPipeline.Tests.PipelineTests.File.SplitSheetsToFiles;

[Collection("FileAccess")]
public class SplitSheetsToFilesTest : SplitSheetsToFilesTestBase
{
    [Theory]
    [InlineData("Split Sheets To Files")]
    public async Task SplitSheetsToFiles_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifySplitSheetsToFiles(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
