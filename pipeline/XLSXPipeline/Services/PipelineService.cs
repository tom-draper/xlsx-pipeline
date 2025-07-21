
namespace XLSXPipeline.Services;

// If you want a PipelineService that orchestrates everything
public interface IPipelineService
{
    Task StartAsync(CancellationToken stoppingToken);
    Task StopAsync();
}

public class PipelineService(
    ILogger<PipelineService> logger,
    IPipelineLoader pipelineLoader,
    ISchedulerService schedulerService,
    IFileWatcherService fileWatcherService) : IPipelineService
{
    private readonly ILogger<PipelineService> _logger = logger;
    private readonly IPipelineLoader _pipelineLoader = pipelineLoader;
    private readonly ISchedulerService _schedulerService = schedulerService;
    private readonly IFileWatcherService _fileWatcherService = fileWatcherService;

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Pipeline Service...");

        try
        {
            // Load all JSON pipeline files
            var (scheduledPipelines, fileWatcherPipelines) = await _pipelineLoader.LoadPipelineFilesAsync(stoppingToken);

            // Start file watchers
            _fileWatcherService.StartFileWatchers(fileWatcherPipelines);

            // Start the scheduler (this will run until cancellation)
            await _schedulerService.RunSchedulerAsync(scheduledPipelines, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Pipeline Service");
            throw;
        }
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping Pipeline Service...");
        
        try
        {
            _fileWatcherService.StopFileWatchers();
            _logger.LogInformation("Pipeline Service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping Pipeline Service");
        }
    }
}
