using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.CreateFile;

public abstract class CreateFileTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<CreateFileAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "CreateFile"), defaultPipelineName)
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

            var action = GetFirstAction(pipelineName);
            if (action.DestinationPath != null)
                AddTempFile(action.DestinationPath.Resolved);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyFileCreated(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var action = GetFirstAction(pipelineName);
        var outputPath = action.DestinationPath!.Resolved;

        Assert.True(System.IO.File.Exists(outputPath), $"Expected output file to exist at: {outputPath}");
        Assert.True(new FileInfo(outputPath).Length > 0, "Expected output file to be non-empty");
    }
}
