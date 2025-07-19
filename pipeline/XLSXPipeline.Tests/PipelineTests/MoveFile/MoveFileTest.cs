namespace XLSXPipeline.Tests.PipelineTests.MoveFile;

[Collection("FileAccess")]
public class MoveFileTest : MoveFileTestBase
{
    [Fact]
    public async Task MoveFilePipeline()
    {
        string pipelineName = "Move File";
        var success = await ExecuteMoveFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Move copy operation should succeed");
    }

    [Fact]
    public async Task MoveFileNoExtensionPipeline()
    {
        string pipelineName = "Move File No Extension";
        var success = await ExecuteMoveFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Move copy operation should succeed");
    }
}