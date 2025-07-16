using System.Text.Json;
using System.Text.RegularExpressions;
using ExcelPipeline.Models;

namespace ExcelPipeline
{
    public partial class Worker(ILogger<Worker> logger) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly List<ScheduledPipeline> _scheduledPipelines = [];
        private readonly List<FileWatcherPipeline> _fileWatcherPipelines = [];
        private readonly string _pipelinesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Pipelines");
        private readonly List<FileSystemWatcher> _fileWatchers = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(_pipelinesDirectory))
            {
                _logger.LogWarning("Pipelines directory not found at: {PipelinesDirectory}", _pipelinesDirectory);
                return;
            }

            // Load all JSON pipeline files
            await LoadPipelineFilesAsync(stoppingToken);

            // Start file watchers
            StartFileWatchers();

            // Start the scheduler loop
            await RunSchedulerAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Clean up file watchers
            foreach (var watcher in _fileWatchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _fileWatchers.Clear();

            await base.StopAsync(cancellationToken);
        }

        private async Task LoadPipelineFilesAsync(CancellationToken stoppingToken)
        {
            try
            {
                var jsonFiles = Directory.GetFiles(_pipelinesDirectory, "*.json");

                if (jsonFiles.Length == 0)
                {
                    _logger.LogWarning("No JSON pipeline files found in: {PipelinesDirectory}", _pipelinesDirectory);
                    return;
                }

                _logger.LogInformation("Found {FileCount} pipeline files to process", jsonFiles.Length);

                foreach (var filePath in jsonFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(filePath, stoppingToken);
                        var pipeline = JsonSerializer.Deserialize<Pipeline>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (pipeline != null)
                        {
                            var triggerType = pipeline.Trigger.Type?.ToLowerInvariant() ?? "once";

                            if (triggerType.Contains("when a file is created"))
                            {
                                var fileWatcherPipeline = new FileWatcherPipeline
                                {
                                    Pipeline = pipeline,
                                    FilePath = filePath,
                                    WatchPath = pipeline.Trigger.Path
                                };
                                _fileWatcherPipelines.Add(fileWatcherPipeline);
                                _logger.LogInformation("Loaded file watcher pipeline: {FileName} watching: {WatchPath}",
                                    Path.GetFileName(filePath), pipeline.Trigger.Path);
                            }
                            else
                            {
                                var scheduledPipeline = CreateScheduledPipeline(pipeline, filePath);
                                if (scheduledPipeline != null)
                                {
                                    _scheduledPipelines.Add(scheduledPipeline);
                                    _logger.LogInformation("Loaded scheduled pipeline: {FileName} with trigger: {TriggerType}",
                                        Path.GetFileName(filePath), pipeline.Trigger.Type);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize pipeline from: {FileName}", Path.GetFileName(filePath));
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Invalid JSON in pipeline file: {FileName}", Path.GetFileName(filePath));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading pipeline file: {FileName}", Path.GetFileName(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing pipelines directory: {PipelinesDirectory}", _pipelinesDirectory);
            }
        }

        private void StartFileWatchers()
        {
            foreach (var fileWatcherPipeline in _fileWatcherPipelines)
            {
                try
                {
                    if (!Directory.Exists(fileWatcherPipeline.WatchPath))
                    {
                        _logger.LogWarning("Watch directory does not exist: {WatchPath}", fileWatcherPipeline.WatchPath);
                        continue;
                    }

                    var watcher = new FileSystemWatcher(fileWatcherPipeline.WatchPath)
                    {
                        NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName,
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = false
                    };

                    watcher.Created += async (sender, e) =>
                    {
                        _logger.LogInformation("File created: {FilePath}, triggering pipeline", e.FullPath);
                        try
                        {
                            await ExecutePipelineAsync(fileWatcherPipeline.Pipeline, e.FullPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing file watcher pipeline for: {FilePath}", e.FullPath);
                        }
                    };

                    _fileWatchers.Add(watcher);
                    _logger.LogInformation("Started file watcher for: {WatchPath}", fileWatcherPipeline.WatchPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting file watcher for: {WatchPath}", fileWatcherPipeline.WatchPath);
                }
            }
        }

        private ScheduledPipeline? CreateScheduledPipeline(Pipeline pipeline, string filePath)
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

                    case var t when t.StartsWith("at ") && TryParseDateTime(t.Substring(3), out var specificDateTime):
                        // Handle "At 2024-12-25" or "At 2024-12-25 14:30"
                        scheduledPipeline.NextRunTime = specificDateTime;
                        scheduledPipeline.IsRecurring = false;
                        break;

                    case var t when t.StartsWith("at ") && TryParseTimeOfDay(t.Substring(3), out var timeOfDay):
                        // Handle "At 6pm", "At 14:30", etc.
                        scheduledPipeline.NextRunTime = GetNextRunTimeForDaily(timeOfDay);
                        scheduledPipeline.IsRecurring = true;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(1);
                        break;

                    case var t when IsCronExpression(t):
                        // Handle cron expressions like "0 30 14 * * MON-FRI"
                        if (TryParseCronExpression(t, out var nextCronTime))
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
                        // Parse "Every 5 minutes", "Every 2 hours", etc.
                        if (TryParseInterval(t.Substring(6), out var interval))
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
                        // Run on the first day of each month at current time
                        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
                        scheduledPipeline.NextRunTime = nextMonth;
                        scheduledPipeline.IsRecurring = true;
                        scheduledPipeline.RecurrenceInterval = TimeSpan.FromDays(30); // Approximate, will be recalculated
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

        private bool TryParseDateTime(string dateTimeString, out DateTime dateTime)
        {
            dateTime = default;
            dateTimeString = dateTimeString.Trim();

            // Try to parse "YYYY-MM-DD HH:MM" format
            var dateTimeRegex = MyRegex();
            var match = dateTimeRegex.Match(dateTimeString);

            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int year) &&
                    int.TryParse(match.Groups[2].Value, out int month) &&
                    int.TryParse(match.Groups[3].Value, out int day) &&
                    int.TryParse(match.Groups[4].Value, out int hour) &&
                    int.TryParse(match.Groups[5].Value, out int minute))
                {
                    try
                    {
                        dateTime = new DateTime(year, month, day, hour, minute, 0);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            // Try to parse "YYYY-MM-DD" format (default to current time)
            var dateRegex = new Regex(@"^(\d{4})-(\d{1,2})-(\d{1,2})$");
            match = dateRegex.Match(dateTimeString);

            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int year) &&
                    int.TryParse(match.Groups[2].Value, out int month) &&
                    int.TryParse(match.Groups[3].Value, out int day))
                {
                    try
                    {
                        var now = DateTime.Now;
                        dateTime = new DateTime(year, month, day, now.Hour, now.Minute, now.Second);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool IsCronExpression(string expression)
        {
            // Basic check for cron expression format (5 or 6 parts separated by spaces)
            var parts = expression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 5 && parts.Length <= 6;
        }

        private bool TryParseCronExpression(string cronExpression, out DateTime nextRunTime)
        {
            nextRunTime = default;

            try
            {
                var parts = cronExpression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5 || parts.Length > 6)
                    return false;

                // Parse cron parts (supports both 5 and 6 field formats)
                var secondsPart = parts.Length == 6 ? parts[0] : "0";
                var minutesPart = parts.Length == 6 ? parts[1] : parts[0];
                var hoursPart = parts.Length == 6 ? parts[2] : parts[1];
                var daysPart = parts.Length == 6 ? parts[3] : parts[2];
                var monthsPart = parts.Length == 6 ? parts[4] : parts[3];
                var dayOfWeekPart = parts.Length == 6 ? parts[5] : parts[4];

                var now = DateTime.Now;
                var nextTime = now.AddMinutes(1); // Start checking from next minute

                // Simple cron parser - find next valid time within the next 7 days
                for (int dayOffset = 0; dayOffset < 7; dayOffset++)
                {
                    var checkDate = now.Date.AddDays(dayOffset);

                    // Check if day of week matches
                    if (!IsValidDayOfWeek(checkDate.DayOfWeek, dayOfWeekPart))
                        continue;

                    // Check if month matches
                    if (!IsValidMonth(checkDate.Month, monthsPart))
                        continue;

                    // Check if day matches
                    if (!IsValidDay(checkDate.Day, daysPart))
                        continue;

                    // Find valid hours and minutes for this day
                    for (int hour = 0; hour < 24; hour++)
                    {
                        if (!IsValidHour(hour, hoursPart))
                            continue;

                        for (int minute = 0; minute < 60; minute++)
                        {
                            if (!IsValidMinute(minute, minutesPart))
                                continue;

                            var candidateTime = new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, hour, minute, 0);
                            if (candidateTime > now)
                            {
                                nextRunTime = candidateTime;
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidDayOfWeek(DayOfWeek dayOfWeek, string cronPart)
        {
            if (cronPart == "*")
                return true;

            var dayNumber = (int)dayOfWeek; // 0 = Sunday, 1 = Monday, etc.

            // Handle ranges like MON-FRI
            if (cronPart.Contains("-"))
            {
                var range = cronPart.Split('-');
                if (range.Length == 2)
                {
                    var start = ParseDayOfWeek(range[0]);
                    var end = ParseDayOfWeek(range[1]);
                    if (start != -1 && end != -1)
                    {
                        return dayNumber >= start && dayNumber <= end;
                    }
                }
            }

            // Handle specific days
            if (int.TryParse(cronPart, out int specificDay))
            {
                return dayNumber == specificDay;
            }

            // Handle named days
            return dayNumber == ParseDayOfWeek(cronPart);
        }

        private int ParseDayOfWeek(string dayName)
        {
            return dayName.ToUpperInvariant() switch
            {
                "SUN" or "SUNDAY" => 0,
                "MON" or "MONDAY" => 1,
                "TUE" or "TUESDAY" => 2,
                "WED" or "WEDNESDAY" => 3,
                "THU" or "THURSDAY" => 4,
                "FRI" or "FRIDAY" => 5,
                "SAT" or "SATURDAY" => 6,
                _ => int.TryParse(dayName, out int day) ? day : -1
            };
        }

        private bool IsValidMonth(int month, string cronPart)
        {
            if (cronPart == "*")
                return true;

            if (int.TryParse(cronPart, out int specificMonth))
            {
                return month == specificMonth;
            }

            return false;
        }

        private bool IsValidDay(int day, string cronPart)
        {
            if (cronPart == "*")
                return true;

            if (int.TryParse(cronPart, out int specificDay))
            {
                return day == specificDay;
            }

            return false;
        }

        private bool IsValidHour(int hour, string cronPart)
        {
            if (cronPart == "*")
                return true;

            // Handle ranges like 9-17
            if (cronPart.Contains("-"))
            {
                var range = cronPart.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                {
                    return hour >= start && hour <= end;
                }
            }

            if (int.TryParse(cronPart, out int specificHour))
            {
                return hour == specificHour;
            }

            return false;
        }

        private static bool IsValidMinute(int minute, string cronPart)
        {
            if (cronPart == "*")
                return true;

            // Handle step values like */15
            if (cronPart.StartsWith("*/"))
            {
                if (int.TryParse(cronPart.Substring(2), out int step))
                {
                    return minute % step == 0;
                }
            }

            if (int.TryParse(cronPart, out int specificMinute))
            {
                return minute == specificMinute;
            }

            return false;
        }

        private bool TryParseTimeOfDay(string timeString, out TimeSpan timeOfDay)
        {
            timeOfDay = default;
            timeString = timeString.Trim().ToLowerInvariant();

            // Handle formats like "6pm", "14:30", "9:15am"
            var timeRegex = new Regex(@"^(\d{1,2})(?::(\d{2}))?\s*(am|pm)?$");
            var match = timeRegex.Match(timeString);

            if (!match.Success)
                return false;

            if (!int.TryParse(match.Groups[1].Value, out int hours))
                return false;

            int minutes = 0;
            if (match.Groups[2].Success && !int.TryParse(match.Groups[2].Value, out minutes))
                return false;

            var ampm = match.Groups[3].Value;

            if (!string.IsNullOrEmpty(ampm))
            {
                if (ampm == "pm" && hours != 12)
                    hours += 12;
                else if (ampm == "am" && hours == 12)
                    hours = 0;
            }

            if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
                return false;

            timeOfDay = new TimeSpan(hours, minutes, 0);
            return true;
        }

        private bool TryParseInterval(string intervalString, out TimeSpan interval)
        {
            interval = default;
            intervalString = intervalString.Trim().ToLowerInvariant();

            // Handle formats like "5 minutes", "2 hours", "1 day"
            var intervalRegex = new Regex(@"^(\d+)\s*(second|minute|hour|day)s?$");
            var match = intervalRegex.Match(intervalString);

            if (!match.Success)
                return false;

            if (!int.TryParse(match.Groups[1].Value, out int value))
                return false;

            var unit = match.Groups[2].Value;

            interval = unit switch
            {
                "second" => TimeSpan.FromSeconds(value),
                "minute" => TimeSpan.FromMinutes(value),
                "hour" => TimeSpan.FromHours(value),
                "day" => TimeSpan.FromDays(value),
                _ => default
            };

            return interval != default;
        }

        private DateTime GetNextRunTimeForDaily(TimeSpan timeOfDay)
        {
            var now = DateTime.Now;
            var today = now.Date.Add(timeOfDay);

            // If the time has already passed today, schedule for tomorrow
            return today > now ? today : today.AddDays(1);
        }

        private async Task RunSchedulerAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting scheduler with {ScheduledCount} scheduled pipelines and {FileWatcherCount} file watchers",
                _scheduledPipelines.Count, _fileWatcherPipelines.Count);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var pipelinesToRun = _scheduledPipelines.Where(p => p.NextRunTime <= now).ToList();

                foreach (var scheduledPipeline in pipelinesToRun)
                {
                    try
                    {
                        await ExecutePipelineAsync(scheduledPipeline.Pipeline);

                        // Update next run time for recurring pipelines
                        if (scheduledPipeline.IsRecurring)
                        {
                            if (scheduledPipeline.IsMonthly)
                            {
                                // For monthly, calculate next month
                                var currentNext = scheduledPipeline.NextRunTime;
                                scheduledPipeline.NextRunTime = currentNext.AddMonths(1);
                            }
                            else if (scheduledPipeline.IsCron)
                            {
                                // For cron expressions, calculate next run time
                                if (TryParseCronExpression(scheduledPipeline.CronExpression, out var nextCronTime))
                                {
                                    scheduledPipeline.NextRunTime = nextCronTime;
                                }
                                else
                                {
                                    _logger.LogWarning("Failed to calculate next cron time for: {FileName}",
                                        Path.GetFileName(scheduledPipeline.FilePath));
                                }
                            }
                            else
                            {
                                scheduledPipeline.NextRunTime = now.Add(scheduledPipeline.RecurrenceInterval);
                            }

                            _logger.LogInformation("Pipeline {FileName} rescheduled for: {NextRunTime}",
                                Path.GetFileName(scheduledPipeline.FilePath), scheduledPipeline.NextRunTime);
                        }
                        else
                        {
                            // Remove one-time pipelines after execution
                            _scheduledPipelines.Remove(scheduledPipeline);
                            _logger.LogInformation("One-time pipeline {FileName} completed and removed",
                                Path.GetFileName(scheduledPipeline.FilePath));
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

        private async Task ExecutePipelineAsync(Pipeline pipeline, string? triggeredFilePath = null)
        {
            _logger.LogInformation("Executing pipeline with {ActionCount} actions", pipeline.Actions?.Count ?? 0);

            if (pipeline.Actions != null)
            {
                foreach (var action in pipeline.Actions)
                {
                    _logger.LogInformation("Executing action: {Action}", action.Type);
                    // Use the triggered file path if available, otherwise use the pipeline trigger path
                    var actionPath = triggeredFilePath ?? pipeline.Trigger.Path;
                    await action.ExecuteAsync(actionPath);
                }
            }
        }

        private class ScheduledPipeline
        {
            public Pipeline Pipeline { get; set; } = null!;
            public string FilePath { get; set; } = string.Empty;
            public string TriggerType { get; set; } = string.Empty;
            public DateTime NextRunTime { get; set; }
            public bool IsRecurring { get; set; }
            public TimeSpan RecurrenceInterval { get; set; }
            public bool IsMonthly { get; set; }
            public bool IsCron { get; set; }
            public string CronExpression { get; set; } = string.Empty;
        }

        private class FileWatcherPipeline
        {
            public Pipeline Pipeline { get; set; } = null!;
            public string FilePath { get; set; } = string.Empty;
            public string WatchPath { get; set; } = string.Empty;
        }

        [GeneratedRegex(@"^(\d{4})-(\d{1,2})-(\d{1,2})\s+(\d{1,2}):(\d{2})$")]
        private static partial Regex MyRegex();
    }
}