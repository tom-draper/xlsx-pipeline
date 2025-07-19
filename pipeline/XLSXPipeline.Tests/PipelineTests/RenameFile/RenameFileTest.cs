namespace XLSXPipeline.Tests.PipelineTests.RenameFile;

[Collection("FileAccess")]
public class CopyRowTest : CopyRowTestBase
{
    [Fact]
    public async Task RenameFilePipeline()
    {
        string pipelineName = "Rename File";
        var success = await ExecuteRenameFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Rename file operation should succeed");
    }

    [Fact]
    public async Task RenameFileNoExtensionPipeline()
    {
        string pipelineName = "Rename File No Extension";
        var success = await ExecuteRenameFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Rename file operation should succeed");
    }
}