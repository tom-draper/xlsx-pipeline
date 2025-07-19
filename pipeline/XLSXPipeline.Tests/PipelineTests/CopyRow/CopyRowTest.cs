namespace XLSXPipeline.Tests.PipelineTests.CopyRow;

[Collection("FileAccess")]
public class CopyColumnTest : CopyColumnTestBase
{
    [Fact]
    public async Task CopyRowPipeline()
    {
        string pipelineName = "Copy Row";
        var success = await ExecuteCopyRowTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Copy row operation should succeed");
    }
}