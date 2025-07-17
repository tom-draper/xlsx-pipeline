using ExcelPipeline.Actions;

namespace ExcelPipeline.Models
{
    public class Pipeline
    {
        public string PipelineName { get; set; }
        public Trigger Trigger { get; set; }
        public List<ActionBase> Actions { get; set; }
    }

    public class Trigger
    {
        public string Type { get; set; }
        public string Path { get; set; }
    }

    public class ScheduledPipeline
    {
        public Pipeline Pipeline { get; set; } = null!;
        public string FilePath { get; set; } = string.Empty;
        public string TriggerType { get; set; } = string.Empty;
        public DateTime NextRunTime { get; set; }
        public bool IsRecurring { get; set; }
        public TimeSpan RecurrenceInterval { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsWeekdaysOnly { get; set; }
        public bool IsCron { get; set; }
        public string CronExpression { get; set; } = string.Empty;
        public bool IsQuarterly { get; set; }
        public bool IsYearly { get; set; }
        public bool IsWeekly { get; set; }
        public bool IsWeekendsOnly { get; set; }
    }

    public class FileWatcherPipeline
    {
        public Pipeline Pipeline { get; set; } = null!;
        public string FilePath { get; set; } = string.Empty;
        public string WatchPath { get; set; } = string.Empty;
    }
}
