
using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.RenameFile;

public abstract class RenameFileTestBase : SpecializedPipelineTestBase<RenameFileAction>
{
    protected RenameFileTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "RenameFile"), defaultPipelineName)
    {
    }

    protected string GetNewName(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var action = GetFirstAction(pipelineName);
        return action.NewName ?? throw new InvalidOperationException($"No NewName found in RenameFileAction for pipeline '{pipelineName}'");
    }

    protected string GetOutputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);
        var newName = GetNewName(pipelineName);

        string filename = newName;
        if (string.IsNullOrEmpty(Path.GetExtension(newName)))
        {
            var originalExtension = Path.GetExtension(inputPath);
            filename = Path.ChangeExtension(newName, originalExtension);
        }

        return Path.Combine(Path.GetDirectoryName(inputPath)!, filename);
    }

    protected (string InputPath, string OutputPath, string NewName) GetRenameInfo(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);
        var newName = GetNewName(pipelineName);

        return (inputPath, outputPath, newName);
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
        catch (Exception ex)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyRenamedFile(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);

        if (!File.Exists(outputPath))
            throw new FileNotFoundException($"Renamed file should exist for pipeline '{pipelineName}'.");

        var outputFileInfo = new FileInfo(outputPath);
        if (outputFileInfo.Length == 0)
            throw new InvalidOperationException($"Renamed file should not be empty for pipeline '{pipelineName}'.");

        // Verify the original file no longer exists (unless it's the same path)
        if (!string.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase))
        {
            if (File.Exists(inputPath))
                throw new InvalidOperationException($"Original file should no longer exist after rename for pipeline '{pipelineName}'.");
        }
    }
}