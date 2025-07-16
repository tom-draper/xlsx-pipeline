using ExcelPipeline.Models;
using ExcelPipeline.Services;

namespace ExcelPipeline.Services
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
            var triggerType = pipeline.Trigger.Type?.ToLowerInvariant() ?? "once";
            var now = DateTime.Now;

            try
            {
                var scheduledPipeline = new ScheduledPipeline
                {
                    Pipeline = pipeline,
                    FilePath = filePath,
                    TriggerType = triggerType
                };

                switch (triggerType)
                {
                    case "once":
                        scheduledPipeline.NextRunTime = now;
                        scheduledPipeline.IsRecurring = false;
                        break;

                    case var t when t.StartsWith("at ") && _triggerParser.TryParseDateTime(t.Substring(3), out var specificDateTime):
                        scheduledPipeline.NextRunTime = specificDateTime;
                        scheduledPipeline.IsRecurring = false;
                        break;

                    case var t when t.StartsWith("at ") && _triggerParser.TryParseTimeOfDay(t.Substring(3), out var timeOfDay):
                        scheduledPipeline.NextRunTime = _triggerParser.GetNextRunTimeForDaily(timeOfDay);
                        scheduledPipeline.IsRecurring = true;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
                        break;

                    case var t when _triggerParser.IsCronExpression(t):
                        if (_triggerParser.TryParseCronExpression(t, out var nextCronTime))
                        {
                            scheduledPipeline.NextRunTime = nextCronTime;
                            scheduledPipeline.IsRecurring = true;
                            scheduledPipeline.IsCron = true;
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
                            scheduledPipeline.IsRecurring = true;
                            scheduledPipeline.RecurrenceInterval = interval;
                        }
                        else
                        {
                            _logger.LogWarning("Invalid interval format in trigger: {TriggerType} for file: {FileName}",
                                triggerType, Path.GetFileName(filePath));
                            return null;
                        }
                        break;

                    case "once a month":
                        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                        scheduledPipeline.NextRunTime = nextMonth;
                        scheduledPipeline.IsRecurring = true;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(30);
                        scheduledPipeline.IsMonthly = true;
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
    }
}