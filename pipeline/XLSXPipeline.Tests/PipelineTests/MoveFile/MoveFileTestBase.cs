using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.MoveFile;

public abstract class MoveFileTestBase : PipelineTestBase
{
    protected readonly string DefaultPipelineName;

    protected MoveFileTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "MoveFile"))
    {
        DefaultPipelineName = defaultPipelineName ?? GetFirstMoveFilePipelineName();
    }

    protected override void UpdatePipelinePaths(Pipeline pipeline)
    {
        UpdatePipelinePathsForAction<MoveFileAction>(pipeline);
    }

    /// <summary>
    /// Gets the output path (destination path) for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The full destination path for the pipeline</returns>
    protected string GetOutputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        var outputPath = pipeline.Actions
            .OfType<MoveFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.DestinationPath))?
            .DestinationPath ?? throw new InvalidOperationException($"No MoveFileAction with destination path found in pipeline '{pipelineName}'");

        if (!outputPath.EndsWith(".xlsx"))
            outputPath += ".xlsx";

        return outputPath;
    }


    /// <summary>
    /// Gets the input path for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The full input path for the pipeline</returns>
    protected string GetInputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        return base.GetInputPath(pipelineName);
    }

    /// <summary>
    /// Executes the MoveFile pipeline test
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline to execute. If null, uses the default pipeline.</param>
    /// <returns>True if the output file was created successfully</returns>
    protected async Task<bool> ExecuteMoveFileTestAsync(string? pipelineName = null)
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

            var pipelineExecutor = await GetPipelineExecutorAsync();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);

            return File.Exists(outputPath);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    /// <summary>
    /// Executes all MoveFile pipelines
    /// </summary>
    /// <returns>Dictionary of pipeline names and their execution results</returns>
    protected async Task<Dictionary<string, bool>> ExecuteAllMoveFileTestsAsync()
    {
        var results = new Dictionary<string, bool>();
        var moveFilePipelines = GetMoveFilePipelineNames();

        foreach (var pipelineName in moveFilePipelines)
        {
            results[pipelineName] = await ExecuteMoveFileTestAsync(pipelineName);
        }

        return results;
    }

    /// <summary>
    /// Verifies file integrity for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    protected void VerifyFileIntegrity(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var outputPath = GetOutputPath(pipelineName);

        Assert.True(File.Exists(outputPath), $"Output file should have been created for pipeline '{pipelineName}'.");

        var outputFileInfo = new FileInfo(outputPath);
        Assert.True(outputFileInfo.Length > 0, $"Output file should not be empty for pipeline '{pipelineName}'.");
    }

    /// <summary>
    /// Gets all pipeline names that contain MoveFile actions
    /// </summary>
    /// <returns>Collection of pipeline names with MoveFile actions</returns>
    protected IEnumerable<string> GetMoveFilePipelineNames()
    {
        return Pipelines
            .Where(kvp => kvp.Value.Actions.OfType<MoveFileAction>().Any())
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Gets the first available MoveFile pipeline name
    /// </summary>
    /// <returns>The name of the first MoveFile pipeline</returns>
    /// <exception cref="InvalidOperationException">Thrown when no MoveFile pipelines are found</exception>
    private string GetFirstMoveFilePipelineName()
    {
        var copyFilePipelineName = GetMoveFilePipelineNames().FirstOrDefault();

        if (copyFilePipelineName == null)
        {
            throw new InvalidOperationException("No pipelines with MoveFile actions found. Available pipelines: " +
                                              string.Join(", ", GetPipelineNames()));
        }

        return copyFilePipelineName;
    }

    /// <summary>
    /// Gets the MoveFile action from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The MoveFile action</returns>
    protected MoveFileAction GetMoveFileAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions
            .OfType<MoveFileAction>()
            .FirstOrDefault() ?? throw new InvalidOperationException($"No MoveFileAction found in pipeline '{pipelineName}'");
    }

    /// <summary>
    /// Gets all MoveFile actions from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>Collection of MoveFile actions</returns>
    protected IEnumerable<MoveFileAction> GetMoveFileActions(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions.OfType<MoveFileAction>();
    }
}