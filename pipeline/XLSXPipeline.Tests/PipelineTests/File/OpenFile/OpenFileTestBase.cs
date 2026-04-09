using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.OpenFile;

public abstract class OpenFileTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<OpenFileAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "OpenFile"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyFileStillExists(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);
        Assert.True(System.IO.File.Exists(inputPath), $"Expected file to still exist after open: {inputPath}");
    }
}
