using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.DeleteFile;

public abstract class DeleteFileTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<DeleteFileAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "DeleteFile"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            // Do not register inputPath as temp file — the action will delete it

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            // Clean up input file if it still exists after a failure
            if (System.IO.File.Exists(inputPath))
                System.IO.File.Delete(inputPath);
            throw;
        }
    }

    protected static void VerifyFileDeleted(string filePath)
    {
        Assert.False(System.IO.File.Exists(filePath), $"Expected file to be deleted: {filePath}");
    }
}
