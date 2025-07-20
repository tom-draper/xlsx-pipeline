using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.CopyFile;

public abstract class CopyFileTestBase : PipelineTestBase
{
    protected readonly string DefaultPipelineName;

    protected CopyFileTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "CopyFile"))
    {
        DefaultPipelineName = defaultPipelineName ?? GetFirstCopyFilePipelineName();
    }

    protected override void UpdatePipelinePaths(Pipeline pipeline)
    {
        UpdatePipelinePathsForAction<CopyFileAction>(pipeline);
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
            .OfType<CopyFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.DestinationPath))?
            .DestinationPath ?? throw new InvalidOperationException($"No CopyFileAction with destination path found in pipeline '{pipelineName}'");

        if (!outputPath.EndsWith(".xlsx"))
            outputPath += ".xlsx";

        return outputPath;
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
    /// Executes the CopyFile pipeline test
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline to execute. If null, uses the default pipeline.</param>
    /// <returns>True if the output file was created successfully</returns>
    protected async Task<bool> ExecuteCopyFileTestAsync(string? pipelineName = null)
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
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Input file not found: {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex}");
            return false;
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    /// <summary>
    /// Executes all CopyFile pipelines
    /// </summary>
    /// <returns>Dictionary of pipeline names and their execution results</returns>
    protected async Task<Dictionary<string, bool>> ExecuteAllCopyFileTestsAsync()
    {
        var results = new Dictionary<string, bool>();
        var copyFilePipelines = GetCopyFilePipelineNames();

        foreach (var pipelineName in copyFilePipelines)
        {
            results[pipelineName] = await ExecuteCopyFileTestAsync(pipelineName);
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
        var inputPath = base.GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);

        Assert.True(File.Exists(outputPath), $"Output file should have been created for pipeline '{pipelineName}'.");

        var outputFileInfo = new FileInfo(outputPath);
        Assert.True(outputFileInfo.Length > 0, $"Output file should not be empty for pipeline '{pipelineName}'.");

        ExcelTestHelpers.VerifyFilesAreIdentical(inputPath, outputPath);
    }

    /// <summary>
    /// Gets all pipeline names that contain CopyFile actions
    /// </summary>
    /// <returns>Collection of pipeline names with CopyFile actions</returns>
    protected IEnumerable<string> GetCopyFilePipelineNames()
    {
        return Pipelines
            .Where(kvp => kvp.Value.Actions.OfType<CopyFileAction>().Any())
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Gets the first available CopyFile pipeline name
    /// </summary>
    /// <returns>The name of the first CopyFile pipeline</returns>
    /// <exception cref="InvalidOperationException">Thrown when no CopyFile pipelines are found</exception>
    private string GetFirstCopyFilePipelineName()
    {
        var copyFilePipelineName = GetCopyFilePipelineNames().FirstOrDefault();

        if (copyFilePipelineName == null)
        {
            throw new InvalidOperationException("No pipelines with CopyFile actions found. Available pipelines: " +
                                              string.Join(", ", GetPipelineNames()));
        }

        return copyFilePipelineName;
    }

    /// <summary>
    /// Gets the CopyFile action from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The CopyFile action</returns>
    protected CopyFileAction GetCopyFileAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions
            .OfType<CopyFileAction>()
            .FirstOrDefault() ?? throw new InvalidOperationException($"No CopyFileAction found in pipeline '{pipelineName}'");
    }

    /// <summary>
    /// Gets all CopyFile actions from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>Collection of CopyFile actions</returns>
    protected IEnumerable<CopyFileAction> GetCopyFileActions(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions.OfType<CopyFileAction>();
    }
}