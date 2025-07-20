using Microsoft.Extensions.Logging;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.ConvertToCSV;

public abstract class ConvertToCSVTestBase : PipelineTestBase
{
    protected readonly string DefaultPipelineName;

    protected ConvertToCSVTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "ConvertToCSV"))
    {
        DefaultPipelineName = defaultPipelineName ?? GetFirstConvertToCSVPipelineName();
    }

    protected override void UpdatePipelinePaths(Pipeline pipeline)
    {
        UpdatePipelinePathsForAction<ConvertToCSVAction>(pipeline);
    }

    /// <summary>
    /// Gets the output path for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The full output path for the pipeline</returns>
    protected string GetOutputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        var outputPath = pipeline.Actions
            .OfType<ConvertToCSVAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.OutputPath))?
            .OutputPath ?? throw new InvalidOperationException($"No ConvertToCSVAction with output path found in pipeline '{pipelineName}'");

        if (!outputPath.EndsWith(".csv"))
            outputPath += ".csv";

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
    /// Executes the ConvertToCSV pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline to execute. If null, uses the default pipeline.</param>
    /// <returns>True if the output file was created successfully</returns>
    protected async Task<bool> ExecuteConvertToCSVTestAsync(string? pipelineName = null)
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
    /// Executes all ConvertToCSV pipelines
    /// </summary>
    /// <returns>Dictionary of pipeline names and their execution results</returns>
    protected async Task<Dictionary<string, bool>> ExecuteAllConvertToCSVAsync()
    {
        var results = new Dictionary<string, bool>();
        var convertToCSVPipelines = GetConvertToCSVPipelineNames();

        foreach (var pipelineName in convertToCSVPipelines)
        {
            results[pipelineName] = await ExecuteConvertToCSVTestAsync(pipelineName);
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
    /// Gets all pipeline names that contain ConvertToCSV actions
    /// </summary>
    /// <returns>Collection of pipeline names with ConvertToCSV actions</returns>
    protected IEnumerable<string> GetConvertToCSVPipelineNames()
    {
        return Pipelines
            .Where(kvp => kvp.Value.Actions.OfType<ConvertToCSVAction>().Any())
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Gets the first available ConvertToCSV pipeline name
    /// </summary>
    /// <returns>The name of the first ConvertToCSV pipeline</returns>
    /// <exception cref="InvalidOperationException">Thrown when no ConvertToCSV pipelines are found</exception>
    private string GetFirstConvertToCSVPipelineName()
    {
        var convertToCSVPipelineName = GetConvertToCSVPipelineNames().FirstOrDefault();

        if (convertToCSVPipelineName == null)
        {
            throw new InvalidOperationException("No pipelines with ConvertToCSV actions found. Available pipelines: " +
                                              string.Join(", ", GetPipelineNames()));
        }

        return convertToCSVPipelineName;
    }

    /// <summary>
    /// Gets the ConvertToCSV action from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The ConvertToCSV action</returns>
    protected ConvertToCSVAction GetConvertToCSVAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions
            .OfType<ConvertToCSVAction>()
            .FirstOrDefault() ?? throw new InvalidOperationException($"No ConvertToCSVAction found in pipeline '{pipelineName}'");
    }
}