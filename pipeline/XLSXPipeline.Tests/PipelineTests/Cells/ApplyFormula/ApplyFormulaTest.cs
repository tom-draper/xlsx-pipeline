namespace XLSXPipeline.Tests.PipelineTests.Cells.ApplyFormula;

[Collection("FileAccess")]
public class ApplyFormulaTest : ApplyFormulaTestBase
{
    [Theory]
    [InlineData("Apply Formula")]
    public async Task ApplyFormula_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFormulaApplied("C1", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
