using XLSXPipeline.Models;

namespace XLSXPipeline.Services
{
    public interface ISchedulerService
    {
        Task RunSchedulerAsync(List<ScheduledPipeline> scheduledPipelines, CancellationToken stoppingToken);
    }

    public class SchedulerService(ILogger<SchedulerService> logger, IPipelineExecutor pipelineExecutor, ITriggerParser triggerParser) : ISchedulerService
    {
        private readonly ILogger<SchedulerService> _logger = logger;
        private readonly IPipelineExecutor _pipelineExecutor = pipelineExecutor;
        private readonly ITriggerParser _triggerParser = triggerParser;

        public async Task RunSchedulerAsync(List<ScheduledPipeline> scheduledPipelines, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting scheduler with {ScheduledCount} scheduled pipelines", scheduledPipelines.Count);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var pipelinesToRun = scheduledPipelines.Where(p => p.NextRunTime <= now).ToList();

                foreach (var scheduledPipeline in pipelinesToRun)
                {
                    try
                    {
                        await _pipelineExecutor.ExecutePipelineAsync(scheduledPipeline.Pipeline);

                        // Update next run time for recurring pipelines
                        if (scheduledPipeline.ScheduleType == ScheduleType.Recurring)
                        {
                            UpdateNextRunTime(scheduledPipeline, now);
                            _logger.LogInformation("Pipeline [{FileName}] rescheduled for: {NextRunTime}",
                                scheduledPipeline.Pipeline.PipelineName, scheduledPipeline.NextRunTime);
                        }
                        else
                        {
                            // Remove one-time pipelines after execution
                            scheduledPipelines.Remove(scheduledPipeline);
                            _logger.LogInformation("One-time pipeline [{FileName}] completed and removed",
                                scheduledPipeline.Pipeline.PipelineName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing pipeline: {FileName}",
                            Path.GetFileName(scheduledPipeline.FilePath));
                    }
                }

                // Wait 1 second before checking again
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void UpdateNextRunTime(ScheduledPipeline scheduledPipeline, DateTime now)
        {
            if (scheduledPipeline.ScheduleType == ScheduleType.Monthly)
            {
                // For monthly, calculate next month
                var currentNext = scheduledPipeline.NextRunTime;
                scheduledPipeline.NextRunTime = currentNext.AddMonths(1);
            }
            else if (scheduledPipeline.ScheduleType == ScheduleType.Cron && scheduledPipeline.CronExpression != null)
            {
                // For cron expressions, calculate next run time
                if (_triggerParser.TryParseCronExpression(scheduledPipeline.CronExpression, out var nextCronTime))
                {
                    scheduledPipeline.NextRunTime = nextCronTime;
                }
                else
                {
                    _logger.LogWarning("Failed to calculate next cron time for: {FileName}",
                        Path.GetFileName(scheduledPipeline.FilePath));
                }
            }
            else if (scheduledPipeline.RecurrenceInterval.HasValue)
            {
                scheduledPipeline.NextRunTime = now.Add((TimeSpan)scheduledPipeline.RecurrenceInterval);
            }
        }
    }
}