using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

public interface IPipelineExecutor
{
    Task ExecutePipelineAsync(Pipeline pipeline, string? triggeredFilePath = null);
}

public class PipelineExecutor(ILogger<PipelineExecutor> logger, IServiceProvider serviceProvider) : IPipelineExecutor
{
    private readonly ILogger<PipelineExecutor> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

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
                action.Services = _serviceProvider;
                _logger.LogInformation("[{Name}]: Executing action {Index}: {Action}", pipeline.PipelineName, actionIndex, action.Type);

                for (int attempt = 1; attempt <= action.Retries + 1; attempt++)
                {
                    try
                    {
                        var executed = await action.ExecuteAsync(triggerFilePath);
                        if (!executed)
                            _logger.LogInformation("[{Name}]: Skipped action {Index}: {Action}", pipeline.PipelineName, actionIndex, action.Type);
                        break; // success
                    }
                    catch (Exception ex) when (attempt <= action.Retries)
                    {
                        _logger.LogWarning("[{Name}]: Action {Index} ({Type}) failed (attempt {Attempt}/{Total}), retrying in {Delay}s: {Error}",
                            pipeline.PipelineName, actionIndex, action.Type, attempt, action.Retries + 1, action.RetryDelaySeconds, ex.Message);
                        await Task.Delay(TimeSpan.FromSeconds(action.RetryDelaySeconds));
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
    }

    private static string NormalizePathSeparators(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar)
                   .Replace('/', Path.DirectorySeparatorChar);
    }
}