using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

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
        _logger.LogInformation("Starting scheduler with {ScheduledCount} scheduled pipeline(s).", scheduledPipelines.Count);

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
                    if (scheduledPipeline.ScheduleType != ScheduleType.Once)
                    {
                        UpdateNextRunTime(scheduledPipeline, now);
                        _logger.LogInformation("Pipeline '{FileName}' rescheduled for: {NextRunTime}",
                            scheduledPipeline.Pipeline.PipelineName, scheduledPipeline.NextRunTime);
                    }
                    else
                    {
                        // Remove one-time pipelines after execution
                        scheduledPipelines.Remove(scheduledPipeline);
                        _logger.LogInformation("One-time pipeline '{FileName}' completed.",
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