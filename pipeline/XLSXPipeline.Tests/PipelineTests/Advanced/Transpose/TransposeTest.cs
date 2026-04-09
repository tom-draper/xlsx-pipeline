namespace XLSXPipeline.Tests.PipelineTests.Advanced.Transpose;

[Collection("FileAccess")]
public class TransposeTest : TransposeTestBase
{
    [Theory]
    [InlineData("Transpose")]
    public async Task Transpose_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyTransposed(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
