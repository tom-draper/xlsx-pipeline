namespace XLSXPipeline.Tests.PipelineTests.CopyFile;

[Collection("FileAccess")]
public class CopyFileTest : CopyFileTestBase
{
    [Fact]
    public async Task CopyFilePipeline()
    {
        string pipelineName = "Copy File";
        var success = await ExecuteCopyFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Copy file operation should succeed");
    }

    [Fact]
    public async Task CopyFileNoExtensionPipeline()
    {
        string pipelineName = "Copy File No Extension";
        var success = await ExecuteCopyFileTestAsync(pipelineName);

        // Verify results
        Assert.True(success, "Copy file operation should succeed");
    }
}
