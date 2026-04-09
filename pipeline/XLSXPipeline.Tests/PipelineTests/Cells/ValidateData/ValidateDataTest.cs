namespace XLSXPipeline.Tests.PipelineTests.Cells.ValidateData;

[Collection("FileAccess")]
public class ValidateDataTest : ValidateDataTestBase
{
    [Theory]
    [InlineData("Validate Data")]
    public async Task ValidateData_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyDataValidationApplied(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
