using System.Text.Json.Serialization;
using ClosedXML.Excel;
using XLSXPipeline.Services;

namespace XLSXPipeline.Actions;

[JsonConverter(typeof(ActionJsonConverter))]
public abstract class ActionBase
{
    /// <summary>
    /// The action type.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonPropertyName("filePath")]
    public PlaceholderString? FilePath { get; set; }

    /// <summary>
    /// If true, this action is always skipped.
    /// </summary>
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Optional condition to skip the action at runtime.
    /// Supported expressions: "fileExists:&lt;path&gt;", "fileNotExists:&lt;path&gt;",
    /// "dayOfWeek:&lt;days&gt;", "fileOlderThan:&lt;duration&gt;", "cellEquals:&lt;sheet&gt;!&lt;cell&gt;:&lt;value&gt;".
    /// </summary>
    public string? SkipIf { get; set; }

    /// <summary>
    /// Number of times to retry the action on failure (default 0 = no retries).
    /// </summary>
    public int Retries { get; set; } = 0;

    /// <summary>
    /// Delay in seconds between retry attempts (default 5.0).
    /// </summary>
    public double RetryDelaySeconds { get; set; } = 5.0;

    /// <summary>
    /// Service provider injected by PipelineExecutor before execution. Not serialized.
    /// </summary>
    [JsonIgnore]
    public IServiceProvider? Services { get; set; }

    /// <summary>
    /// Executes the action with the provided file path from the pipeline.
    /// Returns true if the action ran, false if it was skipped.
    /// </summary>
    public async Task<bool> ExecuteAsync(string triggerFilePath)
    {
        if (Disabled)
            return false;

        if (!string.IsNullOrWhiteSpace(SkipIf) && ShouldSkip(SkipIf, triggerFilePath))
            return false;

        // Determine which file path to use - override takes precedence
        var effectiveFilePath = GetEffectiveFilePath(triggerFilePath);

        if (effectiveFilePath == null)
            throw new ArgumentNullException(nameof(effectiveFilePath));

        // Call the concrete implementation
        await ExecuteInternalAsync(effectiveFilePath);
        return true;
    }

    private static bool ShouldSkip(string skipIf, string filePath)
    {
        const string fileExistsPrefix = "fileExists:";
        const string fileNotExistsPrefix = "fileNotExists:";
        const string dayOfWeekPrefix = "dayOfWeek:";
        const string fileOlderThanPrefix = "fileOlderThan:";
        const string cellEqualsPrefix = "cellEquals:";

        if (skipIf.StartsWith(fileExistsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var path = Helpers.ReplaceDateTimePlaceholders(skipIf[fileExistsPrefix.Length..]);
            return System.IO.File.Exists(path);
        }

        if (skipIf.StartsWith(fileNotExistsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var path = Helpers.ReplaceDateTimePlaceholders(skipIf[fileNotExistsPrefix.Length..]);
            return !System.IO.File.Exists(path);
        }

        if (skipIf.StartsWith(dayOfWeekPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var days = skipIf[dayOfWeekPrefix.Length..].Split(',');
            var today = DateTime.Now.DayOfWeek.ToString();
            return !days.Any(d => d.Trim().Equals(today, StringComparison.OrdinalIgnoreCase)
                               || today.StartsWith(d.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (skipIf.StartsWith(fileOlderThanPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var durationStr = skipIf[fileOlderThanPrefix.Length..].Trim();
            var duration = ParseDuration(durationStr);
            if (!System.IO.File.Exists(filePath)) return false;
            var age = DateTime.Now - System.IO.File.GetLastWriteTime(filePath);
            return age > duration;
        }

        if (skipIf.StartsWith(cellEqualsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var spec = skipIf[cellEqualsPrefix.Length..];
            var bangIdx = spec.IndexOf('!');
            var colonIdx = spec.LastIndexOf(':');
            if (bangIdx < 0 || colonIdx <= bangIdx) return false;
            var sheetName = spec[..bangIdx];
            var cellAddress = spec[(bangIdx + 1)..colonIdx];
            var expectedValue = spec[(colonIdx + 1)..];
            try
            {
                using var wb = new XLWorkbook(filePath);
                var ws = wb.Worksheet(sheetName);
                var actual = ws?.Cell(cellAddress)?.GetString() ?? "";
                return actual.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        return false;
    }

    private static TimeSpan ParseDuration(string s)
    {
        s = s.Trim().ToLowerInvariant();
        if (s.EndsWith('h') && double.TryParse(s[..^1], out var h)) return TimeSpan.FromHours(h);
        if (s.EndsWith('m') && double.TryParse(s[..^1], out var m)) return TimeSpan.FromMinutes(m);
        if (s.EndsWith('d') && double.TryParse(s[..^1], out var d)) return TimeSpan.FromDays(d);
        return TimeSpan.Zero;
    }

    /// <summary>
    /// Gets the effective file path, preferring the FilePath override if provided
    /// </summary>
    protected virtual string? GetEffectiveFilePath(string triggerFilePath)
    {
        string effectiveFilePath;
        if (!string.IsNullOrEmpty(FilePath))
            effectiveFilePath = Path.GetFullPath(Helpers.NormalizePathSeparators(FilePath!));
        else
            effectiveFilePath = Helpers.ReplaceDateTimePlaceholders(triggerFilePath);
        return effectiveFilePath;
    }

    /// <summary>
    /// Implement this method in derived classes to define the action's behavior
    /// </summary>
    protected abstract Task ExecuteInternalAsync(string filePath);
}
