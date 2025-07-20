namespace XLSXPipeline.Tests.PipelineTests.ConvertToCSV;

[Collection("FileAccess")]
public class ConvertToCSVTest : ConvertToCSVTestBase
{
    [Fact]
    public async Task ConvertToCSVPipeline()
    {
        string pipelineName = "Convert To CSV";
        var success = await ExecuteConvertToCSVTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Convert to CSV operation should succeed");
    }

    [Fact]
    public async Task ConvertToCSVNoExtensionPipeline()
    {
        string pipelineName = "Convert To CSV No Extension";
        var success = await ExecuteConvertToCSVTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Convert to CSV operation should succeed");
    }

    [Fact]
    public async Task ConvertToCSVNested()
    {
        string pipelineName = "Convert To CSV Nested";
        var success = await ExecuteConvertToCSVTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Convert to CSV operation should succeed");
    }

}