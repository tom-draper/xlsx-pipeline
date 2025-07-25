
namespace XLSXPipeline.Tests.PipelineTests.Data.MoveColumn;

[Collection("FileAccess")]
public class MoveColumnTest : MoveColumnTestBase
{
    [Theory]
    [InlineData("Move Column")]
    public async Task MoveColumn_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyMoveColumn(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
