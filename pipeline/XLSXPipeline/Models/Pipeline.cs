using XLSXPipeline.Actions;

namespace XLSXPipeline.Models
{
    public class Pipeline
    {
        public required string PipelineName { get; set; }
        public required Trigger Trigger { get; set; }
        public required List<ActionBase> Actions { get; set; }
    }

    public class Trigger
    {
        public required string Type { get; set; }
        public required string Path { get; set; }
    }

    public static class TriggerTypes
    {
        public const string OnChange = "onchange";
        public const string OnNewFile = "onnewfile";
        public const string Once = "once";
        public const string Scheduled = "scheduled";
    }

    public enum ScheduleType
    {
        Once,
        Recurring,
        Cron,
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        WeekdaysOnly,
        WeekendsOnly
    }

    public class ScheduledPipeline
    {
        public Pipeline Pipeline { get; set; } = null!;
        public string FilePath { get; set; } = string.Empty;
        public DateTime NextRunTime { get; set; }
        public ScheduleType ScheduleType { get; set; }
        public TimeSpan? RecurrenceInterval { get; set; }
        public string? CronExpression { get; set; }
    }

    public class FileWatcherPipeline
    {
        public Pipeline Pipeline { get; set; } = null!;
        public string FilePath { get; set; } = string.Empty;
        public string WatchPath { get; set; } = string.Empty;
    }
}
