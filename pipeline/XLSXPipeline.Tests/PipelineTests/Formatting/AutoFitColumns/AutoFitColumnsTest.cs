namespace XLSXPipeline.Tests.PipelineTests.Formatting.AutoFitColumns;

[Collection("FileAccess")]
public class AutoFitColumnsTest : AutoFitColumnsTestBase
{
    [Theory]
    [InlineData("Auto Fit Columns")]
    public async Task AutoFitColumns_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyAutoFitColumns(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
