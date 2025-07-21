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

    private List<ScheduledPipeline> _scheduledPipelines = [];

    // The main long-running task is handled here.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (_scheduledPipelines.Count > 0)
            {
                // The token passed here is automatically cancelled on shutdown.
                await _schedulerService.RunSchedulerAsync(_scheduledPipelines, stoppingToken);
            }
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

    // Handles initial setup before ExecuteAsync is called.
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting pipeline service...");

        // Load pipelines and start file watchers.
        var (scheduledPipelines, fileWatcherPipelines) = await _pipelineLoader.LoadPipelineFilesAsync(cancellationToken);
        _scheduledPipelines = scheduledPipelines;

        if (fileWatcherPipelines != null && fileWatcherPipelines.Count > 0)
        {
            _fileWatcherService.StartFileWatchers(fileWatcherPipelines);
            _logger.LogInformation("File watchers started for {Count} pipelines.", fileWatcherPipelines?.Count ?? 0);
        }

        // Let the base class handle the call to ExecuteAsync.
        await base.StartAsync(cancellationToken);

    }

    // Handles cleanup.
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping pipeline service...");

        _fileWatcherService.StopFileWatchers();
        _logger.LogInformation("File watchers stopped.");

        // The base class will gracefully handle stopping ExecuteAsync.
        await base.StopAsync(cancellationToken);

        _logger.LogInformation("Pipeline service stopped successfully.");
    }
}