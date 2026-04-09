using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

public interface IPipelineExecutor
{
    Task ExecutePipelineAsync(Pipeline pipeline, string? triggeredFilePath = null);
}

public class PipelineExecutor(ILogger<PipelineExecutor> logger) : IPipelineExecutor
{
    private readonly ILogger<PipelineExecutor> _logger = logger;

    public async Task ExecutePipelineAsync(Pipeline pipeline, string? triggeredFilePath = null)
    {
        _logger.LogInformation("Executing pipeline '{Name}' with {ActionCount} action(s).", pipeline.PipelineName, pipeline.Actions?.Count ?? 0);

        if (pipeline.Actions != null)
        {
            var triggerFilePath = triggeredFilePath ?? Path.GetFullPath(NormalizePathSeparators(pipeline.Trigger.Path));

            for (int i = 0; i < pipeline.Actions.Count; i++)
            {
                var action = pipeline.Actions[i];
                var actionIndex = i + 1;
                _logger.LogInformation("[{Name}]: Executing action {Index}: {Action}", pipeline.PipelineName, actionIndex, action.Type);
                try
                {
                    var executed = await action.ExecuteAsync(triggerFilePath);
                    if (!executed)
                        _logger.LogInformation("[{Name}]: Skipped action {Index}: {Action}", pipeline.PipelineName, actionIndex, action.Type);
                }
                catch (Exception ex)
                {
                    var message = $"Pipeline '{pipeline.PipelineName}' failed at action {actionIndex} ({action.Type}): {ex.Message}";
                    _logger.LogError(ex, "Pipeline '{PipelineName}' failed at action {ActionIndex} ({ActionType}).", pipeline.PipelineName, actionIndex, action.Type);
                    throw new InvalidOperationException(message, ex);
                }
            }
        }
    }

    private static string NormalizePathSeparators(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar);
    }
}