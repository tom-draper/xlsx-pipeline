using XLSXPipeline.Models;

namespace XLSXPipeline.Services
{
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
            var triggerType = pipeline.Trigger.Type?.ToLowerInvariant() ?? TriggerTypes.Once;
            var now = DateTime.Now;

            try
            {
                var scheduledPipeline = new ScheduledPipeline
                {
                    Pipeline = pipeline,
                    FilePath = filePath
                };

                switch (triggerType)
                {
                    case TriggerTypes.Once:
                        scheduledPipeline.NextRunTime = now;
                        scheduledPipeline.ScheduleType = ScheduleType.Once;
                        break;

                    case var t when t.StartsWith("at ") && _triggerParser.TryParseDateTime(t.Substring(3), out var specificDateTime):
                        scheduledPipeline.NextRunTime = specificDateTime;
                        scheduledPipeline.ScheduleType = ScheduleType.Once;
                        break;

                    case var t when t.StartsWith("at ") && _triggerParser.TryParseTimeOfDay(t.Substring(3), out var timeOfDay):
                        scheduledPipeline.NextRunTime = _triggerParser.GetNextRunTimeForDaily(timeOfDay);
                        scheduledPipeline.ScheduleType = ScheduleType.Daily;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
                        break;

                    case var t when _triggerParser.IsCronExpression(t):
                        if (_triggerParser.TryParseCronExpression(t, out var nextCronTime))
                        {
                            scheduledPipeline.NextRunTime = nextCronTime;
                            scheduledPipeline.ScheduleType = ScheduleType.Cron;
                            scheduledPipeline.CronExpression = t;
                        }
                        else
                        {
                            _logger.LogWarning("Invalid cron expression: {TriggerType} for file: {FileName}",
                                triggerType, Path.GetFileName(filePath));
                            return null;
                        }
                        break;

                    case var t when t.StartsWith("every "):
                        if (_triggerParser.TryParseInterval(t.Substring(6), out var interval))
                        {
                            scheduledPipeline.NextRunTime = now.Add(interval);
                            scheduledPipeline.ScheduleType = ScheduleType.Recurring;
                            scheduledPipeline.RecurrenceInterval = interval;
                        }
                        else
                        {
                            _logger.LogWarning("Invalid interval format in trigger: {TriggerType} for file: {FileName}",
                                triggerType, Path.GetFileName(filePath));
                            return null;
                        }
                        break;

                    // Daily options
                    case "once a day":
                    case "every day":
                    case "daily":
                        scheduledPipeline.NextRunTime = now.AddDays(1).Date; // Next day at midnight
                        scheduledPipeline.ScheduleType = ScheduleType.Daily;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
                        break;

                    // Weekly options
                    case "once a week":
                    case "every week":
                    case "weekly":
                        scheduledPipeline.NextRunTime = now.AddDays(7);
                        scheduledPipeline.ScheduleType = ScheduleType.Weekly;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(7);
                        break;

                    // Hourly options
                    case "once an hour":
                    case "every hour":
                    case "hourly":
                        scheduledPipeline.NextRunTime = now.AddHours(1);
                        scheduledPipeline.ScheduleType = ScheduleType.Recurring;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromHours(1);
                        break;

                    // Monthly options
                    case "once a month":
                    case "every month":
                    case "monthly":
                        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                        scheduledPipeline.NextRunTime = nextMonth;
                        scheduledPipeline.ScheduleType = ScheduleType.Monthly;
                        break;

                    // Quarterly options
                    case "once a quarter":
                    case "every quarter":
                    case "quarterly":
                        var nextQuarter = GetNextQuarterStart(now);
                        scheduledPipeline.NextRunTime = nextQuarter;
                        scheduledPipeline.ScheduleType = ScheduleType.Quarterly;
                        break;

                    // Yearly options
                    case "once a year":
                    case "every year":
                    case "yearly":
                    case "annually":
                        var nextYear = new DateTime(now.Year + 1, 1, 1);
                        scheduledPipeline.NextRunTime = nextYear;
                        scheduledPipeline.ScheduleType = ScheduleType.Yearly;
                        break;

                    // Weekday options
                    case "weekdays":
                    case "every weekday":
                        var nextWeekday = GetNextWeekday(now);
                        scheduledPipeline.NextRunTime = nextWeekday;
                        scheduledPipeline.ScheduleType = ScheduleType.WeekdaysOnly;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
                        break;

                    // Weekend options
                    case "weekends":
                    case "every weekend":
                        var nextWeekend = GetNextWeekend(now);
                        scheduledPipeline.NextRunTime = nextWeekend;
                        scheduledPipeline.ScheduleType = ScheduleType.WeekendsOnly;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
                        break;

                    default:
                        _logger.LogWarning("Unknown trigger type: {TriggerType} for file: {FileName}",
                            triggerType, Path.GetFileName(filePath));
                        return null;
                }

                return scheduledPipeline;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scheduled pipeline for: {FileName}", Path.GetFileName(filePath));
                return null;
            }
        }

        private static DateTime GetNextQuarterStart(DateTime now)
        {
            var currentQuarter = (now.Month - 1) / 3 + 1;
            var nextQuarterStartMonth = currentQuarter * 3 + 1;

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
            return nextDay.Date;
        }

        private static DateTime GetNextWeekend(DateTime now)
        {
            var nextDay = now.AddDays(1);
            while (nextDay.DayOfWeek != DayOfWeek.Saturday && nextDay.DayOfWeek != DayOfWeek.Sunday)
            {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay.Date;
        }
    }
}