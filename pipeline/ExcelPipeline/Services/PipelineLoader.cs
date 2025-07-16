using System.Text.Json;
using ExcelPipeline.Models;

namespace ExcelPipeline.Services
{
    public interface IPipelineLoader
    {
        Task<(List<ScheduledPipeline>, List<FileWatcherPipeline>)> LoadPipelineFilesAsync(CancellationToken stoppingToken);
    }

    public class PipelineLoader(ILogger<PipelineLoader> logger, IScheduledPipelineFactory scheduledPipelineFactory) : IPipelineLoader
    {
        private readonly ILogger<PipelineLoader> _logger = logger;
        private readonly IScheduledPipelineFactory _scheduledPipelineFactory = scheduledPipelineFactory;
        private readonly string _pipelinesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Pipelines");

        public async Task<(List<ScheduledPipeline>, List<FileWatcherPipeline>)> LoadPipelineFilesAsync(CancellationToken stoppingToken)
        {
            var scheduledPipelines = new List<ScheduledPipeline>();
            var fileWatcherPipelines = new List<FileWatcherPipeline>();

            if (!Directory.Exists(_pipelinesDirectory))
            {
                _logger.LogWarning("Pipelines directory not found at: {PipelinesDirectory}", _pipelinesDirectory);
                return (scheduledPipelines, fileWatcherPipelines);
            }

            try
            {
                var jsonFiles = Directory.GetFiles(_pipelinesDirectory, "*.json");

                if (jsonFiles.Length == 0)
                {
                    _logger.LogWarning("No JSON pipeline files found in: {PipelinesDirectory}", _pipelinesDirectory);
                    return (scheduledPipelines, fileWatcherPipelines);
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
                                fileWatcherPipelines.Add(fileWatcherPipeline);
                                _logger.LogInformation("Loaded file watcher pipeline: {FileName} watching: {WatchPath}",
                                    Path.GetFileName(filePath), pipeline.Trigger.Path);
                            }
                            else
                            {
                                var scheduledPipeline = _scheduledPipelineFactory.CreateScheduledPipeline(pipeline, filePath);
                                if (scheduledPipeline != null)
                                {
                                    scheduledPipelines.Add(scheduledPipeline);
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

            return (scheduledPipelines, fileWatcherPipelines);
        }
    }
}