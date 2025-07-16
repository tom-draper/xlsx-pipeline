using ExcelPipeline.Services;

namespace ExcelPipeline
{
    public partial class Worker(ILogger<Worker> logger, IPipelineLoader pipelineLoader, ISchedulerService schedulerService, IFileWatcherService fileWatcherService) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IPipelineLoader _pipelineLoader = pipelineLoader;
        private readonly ISchedulerService _schedulerService = schedulerService;
        private readonly IFileWatcherService _fileWatcherService = fileWatcherService;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Load all JSON pipeline files
            var (scheduledPipelines, fileWatcherPipelines) = await _pipelineLoader.LoadPipelineFilesAsync(stoppingToken);

            // Start file watchers
            _fileWatcherService.StartFileWatchers(fileWatcherPipelines);

            // Start the scheduler
            await _schedulerService.RunSchedulerAsync(scheduledPipelines, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _fileWatcherService.StopFileWatchers();
            await base.StopAsync(cancellationToken);
        }
    }
}