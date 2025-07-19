using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.Json;
using XLSXPipeline.Extensions;
using XLSXPipeline.Models;
using XLSXPipeline.Services;

namespace XLSXPipeline.Tests.Infrastructure;

public abstract class PipelineTestBase : IDisposable
{
    protected readonly IServiceCollection Services;
    protected readonly IReadOnlyDictionary<string, Pipeline> Pipelines;
    protected readonly string BaseDir;
    private readonly Dictionary<string, Pipeline> _pipelines;
    private readonly List<string> _tempFilesToCleanup;

    protected PipelineTestBase(string testDirectory, string pipelinesSubPath = "Pipelines")
    {
        Services = new ServiceCollection();
        _tempFilesToCleanup = [];
        _pipelines = [];

        ConfigureServices();

        BaseDir = Path.GetFullPath(testDirectory);

        var pipelinesPath = Path.GetFullPath(Path.Combine(BaseDir, pipelinesSubPath));
        LoadPipelinesAsync(pipelinesPath).GetAwaiter().GetResult();

        Pipelines = new ReadOnlyDictionary<string, Pipeline>(_pipelines);

        UpdateAllPipelinesPaths();
    }

    protected virtual void ConfigureServices()
    {
        Services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        Services.AddPipelineServices();
    }

    protected virtual void UpdateAllPipelinesPaths()
    {
        foreach (var pipeline in _pipelines.Values)
        {
            UpdatePipelinePaths(pipeline);
        }
    }

    protected virtual void UpdatePipelinePaths(Pipeline pipeline)
    {
        // Default implementation - can be overridden for specific behavior
    }

    /// <summary>
    /// Updates pipeline paths for actions of the specified type that have a DestinationPath property
    /// </summary>
    /// <typeparam name="T">The action type (e.g., CopyFileAction, MoveFileAction)</typeparam>
    protected void UpdatePipelinePathsForAction<T>(Pipeline pipeline) where T : class
    {
        var actions = pipeline.Actions.OfType<T>();

        foreach (var action in actions)
        {
            UpdatePipelinePropertyForAction(action, "DestinationPath");
            UpdatePipelinePropertyForAction(action, "OutputPath");
        }
    }

    /// <summary>
    /// Updates pipeline paths for actions of the specified type across all pipelines
    /// </summary>
    /// <typeparam name="T">The action type (e.g., CopyFileAction, MoveFileAction)</typeparam>
    protected void UpdateAllPipelinePathsForAction<T>() where T : class
    {
        foreach (var pipeline in _pipelines.Values)
        {
            UpdatePipelinePathsForAction<T>(pipeline);
        }
    }

    protected void UpdatePipelinePropertyForAction<T>(T action, string propertyName) where T : class
    {
        var destinationProperty = typeof(T).GetProperty(propertyName);
        if (destinationProperty != null && destinationProperty.CanWrite)
        {
            var currentDestination = destinationProperty.GetValue(action) as string;
            if (!string.IsNullOrEmpty(currentDestination))
            {
                string fullPath = Path.Combine(BaseDir, currentDestination);
                destinationProperty.SetValue(action, fullPath);
            }
        }
    }

    /// <summary>
    /// Gets a pipeline by name
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline</param>
    /// <returns>The pipeline if found</returns>
    /// <exception cref="KeyNotFoundException">Thrown when pipeline is not found</exception>
    protected Pipeline GetPipeline(string pipelineName)
    {
        if (!Pipelines.TryGetValue(pipelineName, out var pipeline))
        {
            throw new KeyNotFoundException($"Pipeline '{pipelineName}' not found. Available pipelines: {string.Join(", ", Pipelines.Keys)}");
        }
        return pipeline;
    }

    /// <summary>
    /// Tries to get a pipeline by name
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline</param>
    /// <param name="pipeline">The pipeline if found</param>
    /// <returns>True if pipeline was found, false otherwise</returns>
    protected bool TryGetPipeline(string pipelineName, out Pipeline pipeline)
    {
        return Pipelines.TryGetValue(pipelineName, out pipeline);
    }

    /// <summary>
    /// Gets the input path for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline</param>
    /// <returns>The full input path for the pipeline</returns>
    protected string GetInputPath(string pipelineName)
    {
        var pipeline = GetPipeline(pipelineName);
        return Path.GetFullPath(Path.Combine(BaseDir, pipeline.Trigger.Path));
    }

    /// <summary>
    /// Gets all available pipeline names
    /// </summary>
    /// <returns>Collection of pipeline names</returns>
    protected IEnumerable<string> GetPipelineNames()
    {
        return Pipelines.Keys;
    }

    private async Task LoadPipelinesAsync(string pipelinesPath)
    {
        if (!Directory.Exists(pipelinesPath))
        {
            throw new DirectoryNotFoundException($"Pipelines directory not found: {pipelinesPath}");
        }

        var jsonFiles = Directory.GetFiles(pipelinesPath, "*.json", SearchOption.TopDirectoryOnly);

        if (jsonFiles.Length == 0)
        {
            throw new InvalidOperationException($"No JSON pipeline files found in: {pipelinesPath}");
        }

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var pipeline = await CreatePipelineAsync(jsonFile);

                if (_pipelines.ContainsKey(pipeline.PipelineName))
                {
                    throw new InvalidOperationException($"Duplicate pipeline name '{pipeline.PipelineName}' found in file: {jsonFile}");
                }

                _pipelines[pipeline.PipelineName] = pipeline;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load pipeline from {jsonFile}: {ex.Message}", ex);
            }
        }
    }

    protected static async Task<Pipeline> CreatePipelineAsync(string pipelinePath)
    {
        var json = await File.ReadAllTextAsync(pipelinePath);
        var pipeline = JsonSerializer.Deserialize<Pipeline>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (pipeline == null)
        {
            throw new InvalidOperationException($"Failed to deserialize pipeline from: {pipelinePath}");
        }

        return pipeline;
    }

    protected void AddTempFile(string filePath)
    {
        _tempFilesToCleanup.Add(filePath);
    }

    protected IPipelineExecutor GetPipelineExecutor()
    {
        var serviceProvider = Services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IPipelineExecutor>();
    }

    protected async Task CleanupTempFilesAsync()
    {
        await Task.Run(() =>
        {
            foreach (var filePath in _tempFilesToCleanup)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to cleanup temp file {filePath}: {ex.Message}");
                }
            }
            _tempFilesToCleanup.Clear();
        });
    }

    public void Dispose()
    {
        foreach (var filePath in _tempFilesToCleanup)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore cleanup errors in dispose
            }
        }
        _tempFilesToCleanup.Clear();
        GC.SuppressFinalize(this);
    }
}