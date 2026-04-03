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

        foreach (var scheduledPipeline in _currentPipelines)
            _logger.LogInformation("Pipeline '{PipelineName}' scheduled for: {NextRunTime}",
                scheduledPipeline.Pipeline.PipelineName, scheduledPipeline.NextRunTime);

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

    public Task ReloadScheduledPipelinesAsync(List<ScheduledPipeline> pipelines)
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
        return Task.CompletedTask;
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
    try
    {
        switch (scheduledPipeline.ScheduleType)
        {
            case ScheduleType.Recurring:
                if (scheduledPipeline.RecurrenceInterval.HasValue)
                {
                    scheduledPipeline.NextRunTime = now.Add(scheduledPipeline.RecurrenceInterval.Value);
                }
                else
                {
                    _logger.LogWarning("Recurring schedule missing interval for: {FileName}", 
                        Path.GetFileName(scheduledPipeline.FilePath));
                    // Fallback to 1 hour
                    scheduledPipeline.NextRunTime = now.AddHours(1);
                }
                break;

            case ScheduleType.Cron:
                if (!string.IsNullOrEmpty(scheduledPipeline.CronExpression) &&
                    _triggerParser.TryParseCronExpression(scheduledPipeline.CronExpression, out var nextCronTime))
                {
                    scheduledPipeline.NextRunTime = nextCronTime;
                }
                else
                {
                    _logger.LogWarning("Failed to calculate next cron time for: {FileName}", 
                        Path.GetFileName(scheduledPipeline.FilePath));
                    // Fallback to 1 hour from now
                    scheduledPipeline.NextRunTime = now.AddHours(1);
                }
                break;

            case ScheduleType.Daily:
                // Calculate next day at the same time, accounting for potential delays
                var dailyTime = scheduledPipeline.NextRunTime.TimeOfDay;
                var nextDaily = now.Date.Add(dailyTime);
                if (nextDaily <= now)
                    nextDaily = nextDaily.AddDays(1);
                scheduledPipeline.NextRunTime = nextDaily;
                break;

            case ScheduleType.Weekly:
                // Calculate next week at the same day/time, accounting for potential delays
                var weeklyDayOfWeek = scheduledPipeline.NextRunTime.DayOfWeek;
                var weeklyTime = scheduledPipeline.NextRunTime.TimeOfDay;
                var nextWeekly = GetNextWeekdayOccurrence(now, weeklyDayOfWeek, weeklyTime);
                scheduledPipeline.NextRunTime = nextWeekly;
                break;

            case ScheduleType.Monthly:
                // Calculate next month at the same day/time, handling month-end edge cases
                var monthlyDay = scheduledPipeline.NextRunTime.Day;
                var monthlyTime = scheduledPipeline.NextRunTime.TimeOfDay;
                var nextMonthly = GetNextMonthlyOccurrence(now, monthlyDay, monthlyTime);
                scheduledPipeline.NextRunTime = nextMonthly;
                break;

            case ScheduleType.Quarterly:
                // Calculate next quarter at the same day/time
                var quarterlyDay = scheduledPipeline.NextRunTime.Day;
                var quarterlyTime = scheduledPipeline.NextRunTime.TimeOfDay;
                var nextQuarterly = GetNextQuarterlyOccurrence(now, quarterlyDay, quarterlyTime);
                scheduledPipeline.NextRunTime = nextQuarterly;
                break;

            case ScheduleType.Yearly:
                // Calculate next year at the same date/time
                var yearlyMonth = scheduledPipeline.NextRunTime.Month;
                var yearlyDay = scheduledPipeline.NextRunTime.Day;
                var yearlyTime = scheduledPipeline.NextRunTime.TimeOfDay;
                var nextYearly = GetNextYearlyOccurrence(now, yearlyMonth, yearlyDay, yearlyTime);
                scheduledPipeline.NextRunTime = nextYearly;
                break;

            case ScheduleType.WeekdaysOnly:
                var weekdayTime = scheduledPipeline.NextRunTime.TimeOfDay;
                var nextWeekday = GetNextWeekday(now, weekdayTime);
                scheduledPipeline.NextRunTime = nextWeekday;
                break;

            case ScheduleType.WeekendsOnly:
                var weekendTime = scheduledPipeline.NextRunTime.TimeOfDay;
                var nextWeekend = GetNextWeekend(now, weekendTime);
                scheduledPipeline.NextRunTime = nextWeekend;
                break;

            default:
                _logger.LogWarning("Unknown schedule type {ScheduleType} for: {FileName}", 
                    scheduledPipeline.ScheduleType, Path.GetFileName(scheduledPipeline.FilePath));
                // Fallback to 1 hour
                scheduledPipeline.NextRunTime = now.AddHours(1);
                break;
        }

        _logger.LogDebug("Updated next run time for {FileName}: {NextRunTime}", 
            Path.GetFileName(scheduledPipeline.FilePath), scheduledPipeline.NextRunTime);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating next run time for: {FileName}", 
            Path.GetFileName(scheduledPipeline.FilePath));
        // Fallback to 1 hour from now
        scheduledPipeline.NextRunTime = now.AddHours(1);
    }
}

private DateTime GetNextWeekdayOccurrence(DateTime now, DayOfWeek targetDay, TimeSpan targetTime)
{
    var candidate = now.Date.Add(targetTime);
    
    // If today is the target day and time hasn't passed, use today
    if (now.DayOfWeek == targetDay && candidate > now)
        return candidate;
    
    // Otherwise find the next occurrence of this day
    var daysUntilTarget = ((int)targetDay - (int)now.DayOfWeek + 7) % 7;
    if (daysUntilTarget == 0) daysUntilTarget = 7; // Next week
    
    return now.Date.AddDays(daysUntilTarget).Add(targetTime);
}

private DateTime GetNextMonthlyOccurrence(DateTime now, int targetDay, TimeSpan targetTime)
{
    var currentMonth = now.Month;
    var currentYear = now.Year;
    
    // Try current month first
    var candidate = TryCreateDateTime(currentYear, currentMonth, targetDay, targetTime);
    if (candidate.HasValue && candidate.Value > now)
        return candidate.Value;
    
    // Try next month
    var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
    var nextYear = currentMonth == 12 ? currentYear + 1 : currentYear;
    
    candidate = TryCreateDateTime(nextYear, nextMonth, targetDay, targetTime);
    return candidate ?? now.AddMonths(1); // Fallback
}

private DateTime GetNextQuarterlyOccurrence(DateTime now, int targetDay, TimeSpan targetTime)
{
    var currentQuarter = (now.Month - 1) / 3;
    var nextQuarterStartMonth = (currentQuarter + 1) * 3 + 1;
    var nextYear = now.Year;
    
    if (nextQuarterStartMonth > 12)
    {
        nextQuarterStartMonth -= 12;
        nextYear++;
    }
    
    var candidate = TryCreateDateTime(nextYear, nextQuarterStartMonth, targetDay, targetTime);
    return candidate ?? now.AddMonths(3); // Fallback
}

private DateTime GetNextYearlyOccurrence(DateTime now, int targetMonth, int targetDay, TimeSpan targetTime)
{
    var currentYear = now.Year;
    
    // Try current year first
    var candidate = TryCreateDateTime(currentYear, targetMonth, targetDay, targetTime);
    if (candidate.HasValue && candidate.Value > now)
        return candidate.Value;
    
    // Try next year
    candidate = TryCreateDateTime(currentYear + 1, targetMonth, targetDay, targetTime);
    return candidate ?? now.AddYears(1); // Fallback
}

private DateTime GetNextWeekday(DateTime now, TimeSpan targetTime)
{
    var candidate = now.Date.Add(targetTime);
    
    // If today is a weekday and time hasn't passed, use today
    if (IsWeekday(now.DayOfWeek) && candidate > now)
        return candidate;
    
    // Find next weekday
    var nextDay = now.Date.AddDays(1);
    while (!IsWeekday(nextDay.DayOfWeek))
        nextDay = nextDay.AddDays(1);
    
    return nextDay.Add(targetTime);
}

private DateTime GetNextWeekend(DateTime now, TimeSpan targetTime)
{
    var candidate = now.Date.Add(targetTime);
    
    // If today is a weekend and time hasn't passed, use today
    if (IsWeekend(now.DayOfWeek) && candidate > now)
        return candidate;
    
    // Find next weekend
    var nextDay = now.Date.AddDays(1);
    while (!IsWeekend(nextDay.DayOfWeek))
        nextDay = nextDay.AddDays(1);
    
    return nextDay.Add(targetTime);
}

private static bool IsWeekday(DayOfWeek day) => 
    day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;

private static bool IsWeekend(DayOfWeek day) => 
    day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;

private static DateTime? TryCreateDateTime(int year, int month, int day, TimeSpan time)
{
    try
    {
        // Handle cases where the target day doesn't exist in the target month (e.g., Feb 30)
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var actualDay = Math.Min(day, daysInMonth);
        
        return new DateTime(year, month, actualDay).Add(time);
    }
    catch
    {
        return null;
    }
}
}