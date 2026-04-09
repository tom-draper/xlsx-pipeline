using System.Text.Json.Serialization;
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
    /// Supported expressions: "fileExists:&lt;path&gt;", "fileNotExists:&lt;path&gt;".
    /// Paths support datetime placeholders.
    /// </summary>
    public string? SkipIf { get; set; }

    /// <summary>
    /// Executes the action with the provided file path from the pipeline.
    /// Returns true if the action ran, false if it was skipped.
    /// </summary>
    public async Task<bool> ExecuteAsync(string triggerFilePath)
    {
        if (Disabled)
            return false;

        if (!string.IsNullOrWhiteSpace(SkipIf) && ShouldSkip(SkipIf))
            return false;

        // Determine which file path to use - override takes precedence
        var effectiveFilePath = GetEffectiveFilePath(triggerFilePath);

        if (effectiveFilePath == null)
            throw new ArgumentNullException(nameof(effectiveFilePath));

        // Call the concrete implementation
        await ExecuteInternalAsync(effectiveFilePath);
        return true;
    }

    private static bool ShouldSkip(string skipIf)
    {
        const string fileExistsPrefix = "fileExists:";
        const string fileNotExistsPrefix = "fileNotExists:";

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

        return false;
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
