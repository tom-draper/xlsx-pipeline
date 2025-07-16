using ExcelPipeline.Models;

namespace ExcelPipeline.Services
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
                foreach (var action in pipeline.Actions)
                {
                    _logger.LogInformation("Executing action: {Action}", action.Type);
                    // Use the triggered file path if available, otherwise use the pipeline trigger path
                    var actionPath = triggeredFilePath ?? pipeline.Trigger.Path;
                    await action.ExecuteAsync(actionPath);
                }
            }
        }
    }
}