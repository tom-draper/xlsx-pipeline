using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

public class PipelineService(
    ILogger<PipelineService> logger,
    IPipelineLoader pipelineLoader,
    ISchedulerService schedulerService,
    IFileWatcherService fileWatcherService) : BackgroundService
{
    private readonly ILogger<PipelineService> _logger = logger;
    private readonly IPipelineLoader _pipelineLoader = pipelineLoader;
    private readonly ISchedulerService _schedulerService = schedulerService;
    private readonly IFileWatcherService _fileWatcherService = fileWatcherService;
    private readonly string _pipelinesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Pipelines");

    private List<ScheduledPipeline> _scheduledPipelines = [];
    private FileSystemWatcher? _pipelineConfigWatcher;
    private readonly SemaphoreSlim _reloadSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_scheduledPipelines.Count == 0)
            return;

        try
        {
            await _schedulerService.RunSchedulerAsync(_scheduledPipelines, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Scheduler was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in the scheduler.");
        }
        _logger.LogInformation("Scheduler has stopped.");
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting pipeline service...");

        // Load initial pipelines
        await LoadPipelinesAsync(cancellationToken);

        // Start watching for pipeline configuration changes
        //StartPipelineConfigWatcher();

        await base.StartAsync(cancellationToken);

        _logger.LogInformation("Application started. Press Ctrl+C to shut down.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping pipeline service...");

        // Stop configuration watcher
        _pipelineConfigWatcher?.Dispose();

        // Stop file watchers
        _fileWatcherService.StopFileWatchers();
        _logger.LogInformation("File watchers stopped.");

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Pipeline service stopped successfully.");
    }

    private async Task LoadPipelinesAsync(CancellationToken cancellationToken)
    {
        var (scheduledPipelines, fileWatcherPipelines) = await _pipelineLoader.LoadPipelineFilesAsync(cancellationToken);
        _scheduledPipelines = scheduledPipelines;

        if (fileWatcherPipelines != null && fileWatcherPipelines.Count > 0)
        {
            _fileWatcherService.StartFileWatchers(fileWatcherPipelines);
            _logger.LogInformation("File watchers started for {Count} pipeline(s).", fileWatcherPipelines?.Count ?? 0);
        }
    }

    private void StartPipelineConfigWatcher()
    {
        if (!Directory.Exists(_pipelinesDirectory))
        {
            _logger.LogWarning("Pipelines directory not found, cannot watch for changes: {Directory}", _pipelinesDirectory);
            return;
        }

        _pipelineConfigWatcher = new FileSystemWatcher(_pipelinesDirectory)
        {
            Filter = "*.json",
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _pipelineConfigWatcher.Created += OnPipelineConfigChanged;
        _pipelineConfigWatcher.Changed += OnPipelineConfigChanged;
        _pipelineConfigWatcher.Deleted += OnPipelineConfigChanged;
        _pipelineConfigWatcher.Renamed += OnPipelineConfigChanged;

        _logger.LogInformation("Started watching pipeline configuration directory: {Directory}", _pipelinesDirectory);
    }

    private async void OnPipelineConfigChanged(object sender, FileSystemEventArgs e)
    {
        // Use semaphore to prevent multiple simultaneous reloads
        if (!await _reloadSemaphore.WaitAsync(100))
        {
            _logger.LogDebug("Pipeline reload already in progress, skipping.");
            return;
        }

        try
        {
            // Add a small delay to handle rapid file changes (like when saving from an editor)
            await Task.Delay(500);

            _logger.LogInformation("Pipeline configuration changed: {FileName}. Hot-reloading pipelines...", e.Name);

            // Stop current file watchers
            _fileWatcherService.StopFileWatchers();

            // Reload pipelines
            await LoadPipelinesAsync(CancellationToken.None);

            // Note: For scheduled pipelines, you might want to notify the scheduler service
            // to reload its scheduled tasks. This depends on your ISchedulerService implementation.
            await _schedulerService.ReloadScheduledPipelinesAsync(_scheduledPipelines);

            _logger.LogInformation("Pipeline reload completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading pipelines after configuration change.");
        }
        finally
        {
            _reloadSemaphore.Release();
        }
    }

    public override void Dispose()
    {
        _pipelineConfigWatcher?.Dispose();
        _reloadSemaphore?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);    
    }
}