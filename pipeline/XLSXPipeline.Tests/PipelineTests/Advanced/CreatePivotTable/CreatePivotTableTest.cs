namespace XLSXPipeline.Tests.PipelineTests.Advanced.CreatePivotTable;

[Collection("FileAccess")]
public class CreatePivotTableTest : CreatePivotTableTestBase
{
    [Theory]
    [InlineData("Create Pivot Table")]
    public async Task CreatePivotTable_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyPivotTableCreated(pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
