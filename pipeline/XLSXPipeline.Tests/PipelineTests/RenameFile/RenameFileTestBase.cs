using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.RenameFile;

public abstract class RenameFileTestBase : PipelineTestBase
{
    protected readonly string DefaultPipelineName;

    protected RenameFileTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "RenameFile"))
    {
        DefaultPipelineName = defaultPipelineName ?? GetFirstRenameFilePipelineName();
    }

    protected override void UpdatePipelinePaths(Pipeline pipeline)
    {
        UpdatePipelinePathsForAction<RenameFileAction>(pipeline);
    }

    /// <summary>
    /// Gets the new name for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The new name for the file</returns>
    protected string GetRenameFileNewName(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions
            .OfType<RenameFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.NewName))?
            .NewName ?? throw new InvalidOperationException($"No RenameFileAction with NewName found in pipeline '{pipelineName}'");
    }

    /// <summary>
    /// Gets the output path (renamed file path) for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The full output path for the renamed file</returns>
    protected string GetOutputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = base.GetInputPath(pipelineName);
        var newName = GetRenameFileNewName(pipelineName);

        string filename = newName;
        if (string.IsNullOrEmpty(Path.GetExtension(newName)))
        {
            var originalExtension = Path.GetExtension(inputPath);
            filename = Path.ChangeExtension(newName, originalExtension);
        }

        return Path.Combine(Path.GetDirectoryName(inputPath)!, filename);
    }

    /// <summary>
    /// Gets the input path for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The full input path for the pipeline</returns>
    protected new string GetInputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        return base.GetInputPath(pipelineName);
    }

    /// <summary>
    /// Executes the RenameFile pipeline test
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline to execute. If null, uses the default pipeline.</param>
    /// <returns>True if the output file was created successfully</returns>
    protected async Task<bool> ExecuteRenameFileTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = base.GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);
            AddTempFile(outputPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);

            return File.Exists(outputPath);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    /// <summary>
    /// Executes all RenameFile pipelines
    /// </summary>
    /// <returns>Dictionary of pipeline names and their execution results</returns>
    protected async Task<Dictionary<string, bool>> ExecuteAllRenameFileTestsAsync()
    {
        var results = new Dictionary<string, bool>();
        var renameFilePipelines = GetRenameFilePipelineNames();

        foreach (var pipelineName in renameFilePipelines)
        {
            results[pipelineName] = await ExecuteRenameFileTestAsync(pipelineName);
        }

        return results;
    }

    /// <summary>
    /// Verifies the renamed file for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    protected void VerifyRenamedFile(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = base.GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);

        Assert.True(File.Exists(outputPath), $"Renamed file should exist for pipeline '{pipelineName}'.");

        var outputFileInfo = new FileInfo(outputPath);
        Assert.True(outputFileInfo.Length > 0, $"Renamed file should not be empty for pipeline '{pipelineName}'.");

        // Verify the original file no longer exists (unless it's the same path)
        if (!string.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase))
        {
            Assert.False(File.Exists(inputPath), $"Original file should no longer exist after rename for pipeline '{pipelineName}'.");
        }
    }

    /// <summary>
    /// Gets all pipeline names that contain RenameFile actions
    /// </summary>
    /// <returns>Collection of pipeline names with RenameFile actions</returns>
    protected IEnumerable<string> GetRenameFilePipelineNames()
    {
        return Pipelines
            .Where(kvp => kvp.Value.Actions.OfType<RenameFileAction>().Any())
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Gets the first available RenameFile pipeline name
    /// </summary>
    /// <returns>The name of the first RenameFile pipeline</returns>
    /// <exception cref="InvalidOperationException">Thrown when no RenameFile pipelines are found</exception>
    private string GetFirstRenameFilePipelineName()
    {
        var renameFilePipelineName = GetRenameFilePipelineNames().FirstOrDefault();

        if (renameFilePipelineName == null)
        {
            throw new InvalidOperationException("No pipelines with RenameFile actions found. Available pipelines: " +
                                              string.Join(", ", GetPipelineNames()));
        }

        return renameFilePipelineName;
    }

    /// <summary>
    /// Gets the RenameFile action from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The RenameFile action</returns>
    protected RenameFileAction GetRenameFileAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions
            .OfType<RenameFileAction>()
            .FirstOrDefault() ?? throw new InvalidOperationException($"No RenameFileAction found in pipeline '{pipelineName}'");
    }

    /// <summary>
    /// Gets all RenameFile actions from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>Collection of RenameFile actions</returns>
    protected IEnumerable<RenameFileAction> GetRenameFileActions(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions.OfType<RenameFileAction>();
    }

    /// <summary>
    /// Gets the complete rename information for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>Tuple containing input path, output path, and new name</returns>
    protected (string InputPath, string OutputPath, string NewName) GetRenameInfo(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = base.GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);
        var newName = GetRenameFileNewName(pipelineName);

        return (inputPath, outputPath, newName);
    }
}