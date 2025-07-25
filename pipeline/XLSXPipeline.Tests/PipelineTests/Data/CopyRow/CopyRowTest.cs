namespace XLSXPipeline.Tests.PipelineTests.Data.CopyRow;

[Collection("FileAccess")]
public class CopyRowTest : CopyRowTestBase
{
    [Theory]
    [InlineData("Copy Row")]
    public async Task CopyRow_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyCopyRow(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}