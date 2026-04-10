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
    IPipelineDisableService completionService,
    IPipelineRegistry pipelineRegistry) : IPipelineLoader
{
    private readonly ILogger<PipelineLoader> _logger = logger;
    private readonly IScheduledPipelineFactory _scheduledPipelineFactory = scheduledPipelineFactory;
    private readonly IPipelineDisableService _completionService = completionService;
    private readonly IPipelineRegistry _pipelineRegistry = pipelineRegistry;
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

                    if (pipeline != null && Validate(pipeline, filePath))
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
                                WatchPaths = [.. pipeline.Trigger.AllPaths]
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

        // Register all loaded pipelines in the registry
        var allPipelines = scheduledPipelines.Select(sp => sp.Pipeline)
            .Concat(fileWatcherPipelines.Select(fwp => fwp.Pipeline))
            .ToList();
        _pipelineRegistry.Register(allPipelines);

        return (scheduledPipelines, fileWatcherPipelines);
    }

    private bool Validate(Pipeline pipeline, string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        if (string.IsNullOrWhiteSpace(pipeline.PipelineName))
        {
            _logger.LogWarning("Pipeline in '{FileName}' has a null or empty PipelineName. Skipping.", fileName);
            return false;
        }

        if (pipeline.Trigger == null)
        {
            _logger.LogWarning("Pipeline '{PipelineName}' in '{FileName}' has a null Trigger. Skipping.", pipeline.PipelineName, fileName);
            return false;
        }

        if (pipeline.Actions == null)
        {
            _logger.LogWarning("Pipeline '{PipelineName}' in '{FileName}' has a null Actions list. Skipping.", pipeline.PipelineName, fileName);
            return false;
        }

        for (int i = 0; i < pipeline.Actions.Count; i++)
        {
            var action = pipeline.Actions[i];
            if (string.IsNullOrWhiteSpace(action?.Type))
            {
                _logger.LogWarning("Pipeline '{PipelineName}' in '{FileName}' has an action at index {Index} with a null or empty Type. Skipping pipeline.", pipeline.PipelineName, fileName, i);
                return false;
            }
        }

        return true;
    }
}