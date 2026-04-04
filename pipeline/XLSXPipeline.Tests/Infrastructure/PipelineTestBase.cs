using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.Json;
using XLSXPipeline.Actions;
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
        UpdateAllPipelinesPaths(); // Modify pipeline paths to set test directory as base
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
            UpdatePipelinePaths(pipeline);
    }

    protected virtual void UpdatePipelinePaths(Pipeline pipeline)
    {
        // Default implementation - can be overridden for specific behavior
    }

    protected void UpdatePipelinePathsForAction<TAction>(Pipeline pipeline) where TAction : class
    {
        var actions = pipeline.Actions.OfType<TAction>();
        foreach (var action in actions)
        {
            UpdatePipelinePropertyForAction(action, "FilePath");
            UpdatePipelinePropertyForAction(action, "DestinationPath");
            UpdatePipelinePropertyForAction(action, "OutputPath");
            UpdatePipelinePropertyForAction(action, "NewName");
        }
    }

    protected void UpdateAllPipelinePathsForAction<TAction>() where TAction : class
    {
        foreach (var pipeline in _pipelines.Values)
            UpdatePipelinePathsForAction<TAction>(pipeline);
    }

    protected void UpdatePipelinePropertyForAction<TAction>(TAction action, string propertyName) where TAction : class
    {
        var destinationProperty = typeof(TAction).GetProperty(propertyName);
        if (destinationProperty != null && destinationProperty.CanWrite)
        {
            var value = destinationProperty.GetValue(action);
            var currentDestination = value switch
            {
                PlaceholderString ps => ps.Raw,
                string s => s,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(currentDestination))
            {
                // Normalize input path
                var sanitizedDestination = NormalizePathSeparators(currentDestination);

                // If already rooted, don't combine with BaseDir
                string fullPath = Path.IsPathRooted(sanitizedDestination)
                    ? Path.GetFullPath(sanitizedDestination)
                    : Path.GetFullPath(Path.Combine(BaseDir, sanitizedDestination));

                if (destinationProperty.PropertyType == typeof(PlaceholderString))
                    destinationProperty.SetValue(action, (PlaceholderString?)fullPath);
                else
                    destinationProperty.SetValue(action, fullPath);
            }
        }
    }

    private static string NormalizePathSeparators(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar);
    }

    protected Pipeline GetPipeline(string pipelineName)
    {
        if (!Pipelines.TryGetValue(pipelineName, out var pipeline))
            throw new KeyNotFoundException($"Pipeline '{pipelineName}' not found. Available pipelines: {string.Join(", ", Pipelines.Keys)}");
        return pipeline;
    }

    protected bool TryGetPipeline(string pipelineName, out Pipeline pipeline)
    {
        if (Pipelines.TryGetValue(pipelineName, out var foundPipeline))
        {
            pipeline = foundPipeline!;
            return true;
        }

        pipeline = null!;
        return false;
    }

    protected string GetInputPath(string pipelineName)
    {
        var pipeline = GetPipeline(pipelineName);
        var relativePath = NormalizePathSeparators(pipeline.Trigger.Path);
        return Path.GetFullPath(Path.Combine(BaseDir, relativePath));
    }

    protected IEnumerable<string> GetPipelineNames() => Pipelines.Keys;

    protected void AddTempFile(string filePath) => _tempFilesToCleanup.Add(filePath);

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
                        File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to cleanup temp file {filePath}: {ex.Message}");
                }
            }
            _tempFilesToCleanup.Clear();
        });
    }

    private async Task LoadPipelinesAsync(string pipelinesPath)
    {
        if (!Directory.Exists(pipelinesPath))
            throw new DirectoryNotFoundException($"Pipelines directory not found: {pipelinesPath}");

        var jsonFiles = Directory.GetFiles(pipelinesPath, "*.json", SearchOption.TopDirectoryOnly);
        if (jsonFiles.Length == 0)
            throw new InvalidOperationException($"No JSON pipeline files found in: {pipelinesPath}");

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var pipeline = await CreatePipelineAsync(jsonFile);
                if (_pipelines.ContainsKey(pipeline.PipelineName))
                    throw new InvalidOperationException($"Duplicate pipeline name '{pipeline.PipelineName}' found in file: {jsonFile}");
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
            throw new InvalidOperationException($"Failed to deserialize pipeline from: {pipelinePath}");

        return pipeline;
    }

    public void Dispose()
    {
        foreach (var filePath in _tempFilesToCleanup)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
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