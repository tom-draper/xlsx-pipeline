using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

public interface ISchedulerService
{
    Task RunSchedulerAsync(List<ScheduledPipeline> scheduledPipelines, CancellationToken stoppingToken);
    Task ReloadScheduledPipelinesAsync(List<ScheduledPipeline> pipelines);
}

public class SchedulerService(
    ILogger<SchedulerService> logger,
    IPipelineExecutor pipelineExecutor,
    ITriggerParser triggerParser,
    IPipelineDisableService completionService) : ISchedulerService
{
    private readonly ILogger<SchedulerService> _logger = logger;
    private readonly IPipelineExecutor _pipelineExecutor = pipelineExecutor;
    private readonly ITriggerParser _triggerParser = triggerParser;
    private readonly IPipelineDisableService _completionService = completionService;

    private List<ScheduledPipeline> _currentPipelines = [];
    private readonly object _pipelinesLock = new();

    public async Task RunSchedulerAsync(List<ScheduledPipeline> scheduledPipelines, CancellationToken stoppingToken)
    {
        lock (_pipelinesLock)
            _currentPipelines = scheduledPipelines;

        _logger.LogInformation("Starting scheduler with {ScheduledCount} scheduled pipeline(s).", scheduledPipelines.Count);

        while (!stoppingToken.IsCancellationRequested)
        {
            List<ScheduledPipeline> pipelinesToCheck;

            // Get a snapshot of current pipelines to avoid collection modification issues
            lock (_pipelinesLock)
                pipelinesToCheck = _currentPipelines.ToList();

            var now = DateTime.Now;
            var pipelinesToRun = pipelinesToCheck.Where(p => p.NextRunTime <= now).ToList();

            foreach (var scheduledPipeline in pipelinesToRun)
            {
                try
                {
                    await _pipelineExecutor.ExecutePipelineAsync(scheduledPipeline.Pipeline);

                    // Mark "Once" pipelines as completed in their JSON file
                    if (scheduledPipeline.ScheduleType == ScheduleType.Once)
                    {
                        await _completionService.MarkPipelineAsDisabled(scheduledPipeline.FilePath);

                        // Remove from current pipelines list
                        lock (_pipelinesLock)
                            _currentPipelines.Remove(scheduledPipeline);

                        _logger.LogInformation("One-time pipeline '{PipelineName}' completed and marked as disabled.",
                            scheduledPipeline.Pipeline.PipelineName);
                    }
                    else
                    {
                        // Update next run time for recurring pipelines
                        UpdateNextRunTime(scheduledPipeline, now);
                        _logger.LogInformation("Pipeline '{PipelineName}' rescheduled for: {NextRunTime}",
                            scheduledPipeline.Pipeline.PipelineName, scheduledPipeline.NextRunTime);
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

    public async Task ReloadScheduledPipelinesAsync(List<ScheduledPipeline> pipelines)
    {
        _logger.LogInformation("Reloading scheduled pipelines. New count: {Count}", pipelines.Count);

        lock (_pipelinesLock)
        {
            // Preserve next run times for pipelines that haven't changed
            var updatedPipelines = new List<ScheduledPipeline>();

            foreach (var newPipeline in pipelines)
            {
                // Try to find existing pipeline with same file path
                var existingPipeline = _currentPipelines
                    .FirstOrDefault(p => p.FilePath.Equals(newPipeline.FilePath, StringComparison.OrdinalIgnoreCase));

                if (existingPipeline != null &&
                    existingPipeline.ScheduleType == newPipeline.ScheduleType &&
                    PipelineContentUnchanged(existingPipeline, newPipeline))
                {
                    // Keep the existing next run time if pipeline hasn't changed
                    newPipeline.NextRunTime = existingPipeline.NextRunTime;
                    _logger.LogDebug("Preserved next run time for unchanged pipeline: {PipelineName}",
                        newPipeline.Pipeline.PipelineName);
                }

                updatedPipelines.Add(newPipeline);
            }

            _currentPipelines = updatedPipelines;
        }

        _logger.LogInformation("Pipeline reload completed successfully.");
        await Task.CompletedTask;
    }

    private static bool PipelineContentUnchanged(ScheduledPipeline existing, ScheduledPipeline updated)
    {
        // Simple comparison - you could make this more sophisticated
        return existing.Pipeline.PipelineName == updated.Pipeline.PipelineName &&
               existing.CronExpression == updated.CronExpression &&
               existing.RecurrenceInterval == updated.RecurrenceInterval;
    }

    private void UpdateNextRunTime(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        switch (scheduledPipeline.ScheduleType)
        {
            case ScheduleType.Recurring:
                if (scheduledPipeline.RecurrenceInterval.HasValue)
                    scheduledPipeline.NextRunTime = now.Add(scheduledPipeline.RecurrenceInterval.Value);
                break;

            case ScheduleType.Cron:
                if (scheduledPipeline.CronExpression != null &&
                    _triggerParser.TryParseCronExpression(scheduledPipeline.CronExpression, out var nextCronTime))
                    scheduledPipeline.NextRunTime = nextCronTime;
                else
                    _logger.LogWarning("Failed to calculate next cron time for: {FileName}",
                        Path.GetFileName(scheduledPipeline.FilePath));
                break;

            case ScheduleType.Daily:
                scheduledPipeline.NextRunTime = scheduledPipeline.NextRunTime.AddDays(1);
                break;

            case ScheduleType.Weekly:
                scheduledPipeline.NextRunTime = scheduledPipeline.NextRunTime.AddDays(7);
                break;

            case ScheduleType.Monthly:
                scheduledPipeline.NextRunTime = scheduledPipeline.NextRunTime.AddMonths(1);
                break;

            case ScheduleType.Quarterly:
                scheduledPipeline.NextRunTime = scheduledPipeline.NextRunTime.AddMonths(3);
                break;

            case ScheduleType.Yearly:
                scheduledPipeline.NextRunTime = scheduledPipeline.NextRunTime.AddYears(1);
                break;

            case ScheduleType.WeekdaysOnly:
                var nextDay = scheduledPipeline.NextRunTime.AddDays(1);
                while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
                    nextDay = nextDay.AddDays(1);
                scheduledPipeline.NextRunTime = nextDay;
                break;

            case ScheduleType.WeekendsOnly:
                var nextWeekendDay = scheduledPipeline.NextRunTime.AddDays(1);
                while (nextWeekendDay.DayOfWeek != DayOfWeek.Saturday && nextWeekendDay.DayOfWeek != DayOfWeek.Sunday)
                    nextWeekendDay = nextWeekendDay.AddDays(1);
                scheduledPipeline.NextRunTime = nextWeekendDay;
                break;
        }
    }
}