namespace XLSXPipeline.Tests.PipelineTests.Data.UngroupColumns;

[Collection("FileAccess")]
public class UngroupColumnsTest : UngroupColumnsTestBase
{
    [Theory]
    [InlineData("Ungroup Columns")]
    public async Task UngroupColumns_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyUngroupColumns(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
