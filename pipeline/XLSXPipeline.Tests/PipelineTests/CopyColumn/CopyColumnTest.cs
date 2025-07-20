namespace XLSXPipeline.Tests.PipelineTests.CopyColumn;

[Collection("FileAccess")]
public class CopyColumnTest : CopyColumnTestBase
{
    [Theory]
    [InlineData("Copy Column")]
    public async Task CopyColumn_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyCopyColumn(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}