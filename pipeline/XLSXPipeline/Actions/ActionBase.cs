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
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Executes the action with the provided file path from the pipeline
    /// </summary>
    public async Task ExecuteAsync(string triggerFilePath)
    {
        // Determine which file path to use - override takes precedence
        var effectiveFilePath = GetEffectiveFilePath(triggerFilePath);

        // Call the concrete implementation
        await ExecuteInternalAsync(effectiveFilePath);
    }

    /// <summary>
    /// Gets the effective file path, preferring the FilePath override if provided
    /// </summary>
    protected virtual string GetEffectiveFilePath(string triggerFilePath)
    {
        return !string.IsNullOrEmpty(FilePath) ? FilePath : triggerFilePath;
    }

    /// <summary>
    /// Implement this method in derived classes to define the action's behavior
    /// </summary>
    protected abstract Task ExecuteInternalAsync(string filePath);
}