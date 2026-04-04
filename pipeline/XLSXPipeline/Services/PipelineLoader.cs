using System.Text.Json;
using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

public interface IPipelineLoader
{
    Task<(List<ScheduledPipeline>, List<FileWatcherPipeline>)> LoadPipelineFilesAsync(CancellationToken stoppingToken);
}

public class PipelineLoader(
    ILogger<PipelineLoader> logger,
    IScheduledPipelineFactory scheduledPipelineFactory,
    IPipelineDisableService completionService) : IPipelineLoader
{
    private readonly ILogger<PipelineLoader> _logger = logger;
    private readonly IScheduledPipelineFactory _scheduledPipelineFactory = scheduledPipelineFactory;
    private readonly IPipelineDisableService _completionService = completionService;
    private readonly string _pipelinesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Pipelines");

    // Configure JsonSerializerOptions with the source generator context
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        TypeInfoResolver = PipelineJsonContext.Default,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

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

            _logger.LogInformation("Found {FileCount} pipeline(s) files to process.", jsonFiles.Length);

            foreach (var filePath in jsonFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath, stoppingToken);
#pragma warning disable IL2026
                    var pipeline = JsonSerializer.Deserialize<Pipeline>(json, JsonOptions);
#pragma warning restore IL2026

                    if (pipeline != null)
                    {
                        var triggerType = pipeline.Trigger.Type?.ToLowerInvariant() ?? TriggerTypes.Once;

                        // Check if this is a "Once" pipeline that has already been completed
                        if (triggerType == TriggerTypes.Once)
                        {
                            var isDisabled = await _completionService.IsPipelineDisabledAsync(filePath);
                            if (isDisabled)
                            {
                                _logger.LogInformation("Skipping disabled pipeline: {FileName}", Path.GetFileName(filePath));
                                continue;
                            }
                        }

                        if (triggerType == TriggerTypes.OnChange || triggerType == TriggerTypes.OnNewFile)
                        {
                            var fileWatcherPipeline = new FileWatcherPipeline
                            {
                                Pipeline = pipeline,
                                FilePath = filePath,
                                WatchPath = pipeline.Trigger.Path
                            };
                            fileWatcherPipelines.Add(fileWatcherPipeline);
                            _logger.LogInformation("Loaded file watcher pipeline: {FileName}", Path.GetFileName(filePath));
                        }
                        else
                        {
                            var scheduledPipeline = _scheduledPipelineFactory.CreateScheduledPipeline(pipeline, filePath);
                            if (scheduledPipeline != null)
                            {
                                scheduledPipelines.Add(scheduledPipeline);
                                _logger.LogInformation("Loaded scheduled pipeline: {FileName}", Path.GetFileName(filePath));
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