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

    // Backing field for FilePath
    private string? _filePath;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? FilePath
    {
        get => _filePath != null ? Helpers.ReplaceDateTimePlaceholders(_filePath) : null;
        set => _filePath = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("filePath")]
    public string? JsonFilePath
    {
        get => _filePath;
        set => _filePath = value;
    }

    /// <summary>
    /// Executes the action with the provided file path from the pipeline
    /// </summary>
    public async Task ExecuteAsync(string triggerFilePath)
    {
        // Determine which file path to use - override takes precedence
        var effectiveFilePath = GetEffectiveFilePath(triggerFilePath);

        if (effectiveFilePath == null)
            throw new ArgumentNullException(nameof(effectiveFilePath));

        // Call the concrete implementation
        await ExecuteInternalAsync(effectiveFilePath);
    }

    /// <summary>
    /// Gets the effective file path, preferring the FilePath override if provided
    /// </summary>
    protected virtual string? GetEffectiveFilePath(string triggerFilePath)
    {
        string effectiveFilePath;
        if (!string.IsNullOrEmpty(FilePath))
            effectiveFilePath = Path.GetFullPath(Helpers.NormalizePathSeparators(FilePath));
        else
            effectiveFilePath = Helpers.ReplaceDateTimePlaceholders(triggerFilePath);
        return effectiveFilePath;
    }

    /// <summary>
    /// Implement this method in derived classes to define the action's behavior
    /// </summary>
    protected abstract Task ExecuteInternalAsync(string filePath);
}
