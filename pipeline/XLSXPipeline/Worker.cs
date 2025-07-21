using XLSXPipeline.Services;

namespace XLSXPipeline;

public partial class Worker(ILogger<Worker> logger, IPipelineService pipelineService) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IPipelineService _pipelineService = pipelineService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Worker starting...");
            await _pipelineService.StartAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker encountered an error");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker stopping...");
        await _pipelineService.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}