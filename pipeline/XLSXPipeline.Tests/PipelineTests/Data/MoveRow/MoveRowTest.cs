
namespace XLSXPipeline.Tests.PipelineTests.Data.MoveRow;

[Collection("FileAccess")]
public class MoveRowTest : MoveRowTestBase
{
    [Theory]
    [InlineData("Move Row")]
    public async Task MoveRow_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyMoveRow(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
