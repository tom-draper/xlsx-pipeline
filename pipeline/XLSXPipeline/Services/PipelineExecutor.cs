using XLSXPipeline.Models;

namespace XLSXPipeline.Services
{
    public interface IPipelineExecutor
    {
        Task ExecutePipelineAsync(Pipeline pipeline, string? triggeredFilePath = null);
    }

    public class PipelineExecutor(ILogger<PipelineExecutor> logger) : IPipelineExecutor
    {
        private readonly ILogger<PipelineExecutor> _logger = logger;

        public async Task ExecutePipelineAsync(Pipeline pipeline, string? triggeredFilePath = null)
        {
            _logger.LogInformation("Executing pipeline with {ActionCount} actions", pipeline.Actions?.Count ?? 0);

            if (pipeline.Actions != null)
            {
                var currentFilePath = triggeredFilePath ?? pipeline.Trigger.Path;

                foreach (var action in pipeline.Actions)
                {
                    _logger.LogInformation("Executing action: {Action}", action.Type);
                    await action.ExecuteAsync(currentFilePath);
                }
            }
        }
    }
}