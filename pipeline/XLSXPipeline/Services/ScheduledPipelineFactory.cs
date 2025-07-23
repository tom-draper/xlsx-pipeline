using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

public interface IScheduledPipelineFactory
{
    ScheduledPipeline? CreateScheduledPipeline(Pipeline pipeline, string filePath);
}

public class ScheduledPipelineFactory(ILogger<ScheduledPipelineFactory> logger, ITriggerParser triggerParser) : IScheduledPipelineFactory
{
    private readonly ILogger<ScheduledPipelineFactory> _logger = logger;
    private readonly ITriggerParser _triggerParser = triggerParser;

    public ScheduledPipeline? CreateScheduledPipeline(Pipeline pipeline, string filePath)
    {
        var triggerType = pipeline.Trigger.Type?.Trim() ?? TriggerTypes.Once;
        var originalTrigger = triggerType;
        triggerType = triggerType.ToLowerInvariant();
        var now = DateTime.Now;

        try
        {
            var scheduledPipeline = new ScheduledPipeline
            {
                Pipeline = pipeline,
                FilePath = filePath
            };

            // Try parsing in order of specificity
            if (TryParseSpecificDateTime(triggerType, scheduledPipeline, now) ||
                TryParseTimeOfDay(triggerType, scheduledPipeline, now) ||
                TryParseCronExpression(triggerType, scheduledPipeline, originalTrigger) ||
                TryParseNaturalLanguage(triggerType, scheduledPipeline, now) ||
                TryParseInterval(triggerType, scheduledPipeline, now) ||
                TryParseCommonPhrases(triggerType, scheduledPipeline, now))
            {
                _logger.LogDebug("Created scheduled pipeline for {FileName} with trigger '{Trigger}', next run: {NextRun}",
                    Path.GetFileName(filePath), originalTrigger, scheduledPipeline.NextRunTime);
                return scheduledPipeline;
            }

            _logger.LogWarning("Unknown trigger type: {TriggerType} for file: {FileName}",
                originalTrigger, Path.GetFileName(filePath));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating scheduled pipeline for: {FileName}", Path.GetFileName(filePath));
            return null;
        }
    }

    private bool TryParseSpecificDateTime(string triggerType, ScheduledPipeline scheduledPipeline, DateTime now)
    {
        if (triggerType == TriggerTypes.Once)
        {
            scheduledPipeline.NextRunTime = now;
            scheduledPipeline.ScheduleType = ScheduleType.Once;
            return true;
        }

        if (triggerType.StartsWith("at ") && triggerType.Length > 3)
        {
            var dateTimeString = triggerType.Substring(3);
            if (_triggerParser.TryParseDateTime(dateTimeString, out var specificDateTime))
            {
                scheduledPipeline.NextRunTime = specificDateTime;
                scheduledPipeline.ScheduleType = ScheduleType.Once;
                return true;
            }
        }

        return false;
    }

    private bool TryParseTimeOfDay(string triggerType, ScheduledPipeline scheduledPipeline, DateTime now)
    {
        if (triggerType.StartsWith("at ") && triggerType.Length > 3)
        {
            var timeString = triggerType.Substring(3);
            if (_triggerParser.TryParseTimeOfDay(timeString, out var timeOfDay))
            {
                scheduledPipeline.NextRunTime = _triggerParser.GetNextRunTimeForDaily(timeOfDay);
                scheduledPipeline.ScheduleType = ScheduleType.Daily;
                scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
                return true;
            }
        }

        return false;
    }

    private bool TryParseCronExpression(string triggerType, ScheduledPipeline scheduledPipeline, string originalTrigger)
    {
        if (_triggerParser.IsCronExpression(triggerType))
        {
            if (_triggerParser.TryParseCronExpression(triggerType, out var nextCronTime))
            {
                scheduledPipeline.NextRunTime = nextCronTime;
                scheduledPipeline.ScheduleType = ScheduleType.Cron;
                scheduledPipeline.CronExpression = originalTrigger; // Keep original case/formatting
                return true;
            }
            else
            {
                _logger.LogWarning("Invalid cron expression: {TriggerType}", originalTrigger);
            }
        }

        return false;
    }

    private bool TryParseNaturalLanguage(string triggerType, ScheduledPipeline scheduledPipeline, DateTime now)
    {
        if (_triggerParser.TryParseNaturalLanguage(triggerType, out string cronExpression))
        {
            if (_triggerParser.TryParseCronExpression(cronExpression, out var nextCronTime))
            {
                scheduledPipeline.NextRunTime = nextCronTime;
                scheduledPipeline.ScheduleType = ScheduleType.Cron;
                scheduledPipeline.CronExpression = cronExpression;
                return true;
            }
        }

        return false;
    }

    private bool TryParseInterval(string triggerType, ScheduledPipeline scheduledPipeline, DateTime now)
    {
        if (triggerType.StartsWith("every ") && triggerType.Length > 6)
        {
            var intervalString = triggerType.Substring(6);

            // Try parsing as interval (e.g., "every 5 minutes")
            if (_triggerParser.TryParseInterval(intervalString, out var interval))
            {
                scheduledPipeline.NextRunTime = now.Add(interval);
                scheduledPipeline.ScheduleType = ScheduleType.Recurring;
                scheduledPipeline.RecurrenceInterval = interval;
                return true;
            }

            // Handle single unit intervals (e.g., "every minute", "every hour")
            if (ParseSingleUnitInterval(intervalString, out interval))
            {
                scheduledPipeline.NextRunTime = now.Add(interval);
                scheduledPipeline.ScheduleType = ScheduleType.Recurring;
                scheduledPipeline.RecurrenceInterval = interval;
                return true;
            }
        }

        return false;
    }

    private static bool TryParseCommonPhrases(string triggerType, ScheduledPipeline scheduledPipeline, DateTime now)
    {
        return triggerType switch
        {
            // Daily options
            "once a day" or "every day" or "daily" => SetDailySchedule(scheduledPipeline, now),

            // Weekly options  
            "once a week" or "every week" or "weekly" => SetWeeklySchedule(scheduledPipeline, now),

            // Hourly options
            "once an hour" or "every hour" or "hourly" => SetHourlySchedule(scheduledPipeline, now),

            // Monthly options
            "once a month" or "every month" or "monthly" => SetMonthlySchedule(scheduledPipeline, now),

            // Quarterly options
            "once a quarter" or "every quarter" or "quarterly" => SetQuarterlySchedule(scheduledPipeline, now),

            // Yearly options
            "once a year" or "every year" or "yearly" or "annually" => SetYearlySchedule(scheduledPipeline, now),

            // Weekday options
            "weekdays" or "every weekday" => SetWeekdaysSchedule(scheduledPipeline, now),

            // Weekend options
            "weekends" or "every weekend" => SetWeekendsSchedule(scheduledPipeline, now),

            _ => false
        };
    }

    private static bool ParseSingleUnitInterval(string intervalString, out TimeSpan interval)
    {
        interval = intervalString switch
        {
            "second" => TimeSpan.FromSeconds(1),
            "minute" => TimeSpan.FromMinutes(1),
            "hour" => TimeSpan.FromHours(1),
            "day" => TimeSpan.FromDays(1),
            _ => default
        };

        return interval != default;
    }

    private static bool SetDailySchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        // Schedule for next day at the same time
        scheduledPipeline.NextRunTime = now.AddDays(1);
        scheduledPipeline.ScheduleType = ScheduleType.Daily;
        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
        return true;
    }

    private static bool SetWeeklySchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        scheduledPipeline.NextRunTime = now.AddDays(7);
        scheduledPipeline.ScheduleType = ScheduleType.Weekly;
        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(7);
        return true;
    }

    private static bool SetHourlySchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        scheduledPipeline.NextRunTime = now.AddHours(1);
        scheduledPipeline.ScheduleType = ScheduleType.Recurring;
        scheduledPipeline.RecurrenceInterval = TimeSpan.FromHours(1);
        return true;
    }

    private static bool SetMonthlySchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        // Preserve the current day and time, move to next month
        var nextMonth = now.AddMonths(1);
        scheduledPipeline.NextRunTime = nextMonth;
        scheduledPipeline.ScheduleType = ScheduleType.Monthly;
        return true;
    }

    private static bool SetQuarterlySchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        var nextQuarter = GetNextQuarterStart(now);
        scheduledPipeline.NextRunTime = nextQuarter;
        scheduledPipeline.ScheduleType = ScheduleType.Quarterly;
        return true;
    }

    private static bool SetYearlySchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        // Preserve the current month, day, and time, move to next year
        var nextYear = now.AddYears(1);
        scheduledPipeline.NextRunTime = nextYear;
        scheduledPipeline.ScheduleType = ScheduleType.Yearly;
        return true;
    }

    private static bool SetWeekdaysSchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        var nextWeekday = GetNextWeekday(now);
        scheduledPipeline.NextRunTime = nextWeekday;
        scheduledPipeline.ScheduleType = ScheduleType.WeekdaysOnly;
        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
        return true;
    }

    private static bool SetWeekendsSchedule(ScheduledPipeline scheduledPipeline, DateTime now)
    {
        var nextWeekend = GetNextWeekend(now);
        scheduledPipeline.NextRunTime = nextWeekend;
        scheduledPipeline.ScheduleType = ScheduleType.WeekendsOnly;
        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
        return true;
    }

    private static DateTime GetNextQuarterStart(DateTime now)
    {
        var currentQuarter = (now.Month - 1) / 3;
        var nextQuarterStartMonth = (currentQuarter + 1) * 3 + 1;

        if (nextQuarterStartMonth > 12)
        {
            return new DateTime(now.Year + 1, 1, 1);
        }

        return new DateTime(now.Year, nextQuarterStartMonth, 1);
    }

    private static DateTime GetNextWeekday(DateTime now)
    {
        var nextDay = now.AddDays(1);
        while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
        {
            nextDay = nextDay.AddDays(1);
        }
        return nextDay; // Preserve time of day
    }

    private static DateTime GetNextWeekend(DateTime now)
    {
        var nextDay = now.AddDays(1);
        while (nextDay.DayOfWeek != DayOfWeek.Saturday && nextDay.DayOfWeek != DayOfWeek.Sunday)
        {
            nextDay = nextDay.AddDays(1);
        }
        return nextDay; // Preserve time of day
    }
}
