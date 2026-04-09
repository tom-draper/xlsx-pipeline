namespace XLSXPipeline.Tests.PipelineTests.Worksheet.RecalculateFormulas;

[Collection("FileAccess")]
public class RecalculateFormulasTest : RecalculateFormulasTestBase
{
    [Theory]
    [InlineData("Recalculate Formulas")]
    public async Task RecalculateFormulas_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyRecalculated(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
