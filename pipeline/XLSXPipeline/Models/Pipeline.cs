using XLSXPipeline.Actions;

namespace XLSXPipeline.Models;

public class Pipeline
{
    public required string PipelineName { get; set; }
    public required Trigger Trigger { get; set; }
    public required List<ActionBase> Actions { get; set; }
    public bool Disabled { get; set; } = false;
}

public class Trigger
{
    public required string Type { get; set; }
    public required string Path { get; set; }
    public List<string>? Paths { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyList<string> AllPaths =>
        Paths is { Count: > 0 } ? Paths : [Path];
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
    public List<string> WatchPaths { get; set; } = [];
}
