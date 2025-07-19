using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XLSXPipeline.Extensions;
using XLSXPipeline.Models;
using XLSXPipeline.Services;

namespace XLSXPipeline.Tests.Infrastructure;

public abstract class PipelineTestBase : IDisposable
{
    protected readonly IServiceCollection Services;
    protected readonly Pipeline Pipeline;
    protected readonly string InputPath;
    protected readonly string BaseDir;
    private readonly List<string> _tempFilesToCleanup;

    protected PipelineTestBase(string testDirectory, string? pipelineSubPath = null)
    {
        Services = new ServiceCollection();
        _tempFilesToCleanup = [];

        ConfigureServices();

        BaseDir = Path.GetFullPath(testDirectory);

        pipelineSubPath ??= Path.Combine("Pipelines", "Pipeline.json");

        var pipelinePath = Path.GetFullPath(Path.Combine(BaseDir, pipelineSubPath));
        Pipeline = CreatePipelineAsync(pipelinePath).GetAwaiter().GetResult();

        UpdatePipelinePaths();
        InputPath = Path.GetFullPath(Path.Combine(BaseDir, Pipeline.Trigger.Path));
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

    protected virtual void UpdatePipelinePaths()
    {
        // Default implementation - can be overridden for specific behavior
    }

    /// <summary>
    /// Updates pipeline paths for actions of the specified type that have a DestinationPath property
    /// </summary>
    /// <typeparam name="T">The action type (e.g., CopyFileAction, MoveFileAction)</typeparam>
    protected void UpdatePipelinePathsForAction<T>() where T : class
    {
        var actions = Pipeline.Actions.OfType<T>();

        foreach (var action in actions)
        {
            UpdatePipelinePropertyForAction(action, "DestinationPath");
            UpdatePipelinePropertyForAction(action, "OutputPath");
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

    protected static async Task<Pipeline> CreatePipelineAsync(string pipelinePath)
    {
        var json = await File.ReadAllTextAsync(pipelinePath);
        return JsonSerializer.Deserialize<Pipeline>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    protected void AddTempFile(string filePath)
    {
        _tempFilesToCleanup.Add(filePath);
    }

    protected async Task<IPipelineExecutor> GetPipelineExecutorAsync()
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
