using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using XLSXPipeline.Services;

namespace XLSXPipeline.Actions.Advanced;

public class RunPipelineAction : ActionBase
{
    [JsonPropertyName("pipelineName")]
    public required string PipelineName { get; set; }

    /// <summary>
    /// Optional file path to use as the trigger for the target pipeline.
    /// If not set, uses the current pipeline's file path.
    /// </summary>
    [JsonPropertyName("targetFilePath")]
    public PlaceholderString? TargetFilePath { get; set; }

    protected override async Task ExecuteInternalAsync(string filePath)
    {
        var registry = Services?.GetService<IPipelineRegistry>()
            ?? throw new InvalidOperationException("IPipelineRegistry is not available.");
        var executor = Services?.GetService<IPipelineExecutor>()
            ?? throw new InvalidOperationException("IPipelineExecutor is not available.");

        var pipeline = registry.Find(PipelineName)
            ?? throw new InvalidOperationException($"Pipeline '{PipelineName}' not found.");

        string? targetFilePath = !string.IsNullOrEmpty(TargetFilePath) ? (string?)TargetFilePath : filePath;
        await executor.ExecutePipelineAsync(pipeline, targetFilePath);
    }
}
