namespace XLSXPipeline.Tests.PipelineTests.CopyColumn;

[Collection("FileAccess")]
public class CopyColumnTest : CopyColumnTestBase
{
    [Fact]
    public async Task CopyColumnPipeline()
    {
        string pipelineName = "Copy Column";
        var success = await ExecuteCopyColumnTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Copy row operation should succeed");
    }
}