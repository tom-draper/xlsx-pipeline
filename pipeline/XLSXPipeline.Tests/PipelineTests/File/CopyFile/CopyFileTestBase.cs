using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.CopyFile;

public abstract class CopyFileTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<CopyFileAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "CopyFile"), defaultPipelineName)
{
    protected string GetOutputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        var outputPath = pipeline.Actions
            .OfType<CopyFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.DestinationPath))?
            .DestinationPath ?? throw new InvalidOperationException($"No CopyFileAction with destination path found in pipeline '{pipelineName}'");

        if (!outputPath.EndsWith(".xlsx"))
            outputPath += ".xlsx";

        return outputPath;
    }

    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);
            AddTempFile(outputPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyFileIntegrity(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var outputPath = GetOutputPath(pipelineName);

        if (!System.IO.File.Exists(outputPath))
            throw new FileNotFoundException($"Copied file should exist for pipeline '{pipelineName}'.");

        var outputFileInfo = new FileInfo(outputPath);
        if (outputFileInfo.Length == 0)
            throw new InvalidOperationException($"Copied file should not be empty for pipeline '{pipelineName}'.");
    }
}
