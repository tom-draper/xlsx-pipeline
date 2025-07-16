using ExcelPipeline.Models;

namespace ExcelPipeline.Services
{
    public interface IFileWatcherService
    {
        void StartFileWatchers(List<FileWatcherPipeline> fileWatcherPipelines);
        void StopFileWatchers();
    }

    public class FileWatcherService(ILogger<FileWatcherService> logger, IPipelineExecutor pipelineExecutor) : IFileWatcherService
    {
        private readonly ILogger<FileWatcherService> _logger = logger;
        private readonly IPipelineExecutor _pipelineExecutor = pipelineExecutor;
        private readonly List<FileSystemWatcher> _fileWatchers = [];

        public void StartFileWatchers(List<FileWatcherPipeline> fileWatcherPipelines)
        {
            foreach (var fileWatcherPipeline in fileWatcherPipelines)
            {
                try
                {
                    if (!Directory.Exists(fileWatcherPipeline.WatchPath))
                    {
                        _logger.LogWarning("Watch directory does not exist: {WatchPath}", fileWatcherPipeline.WatchPath);
                        continue;
                    }

                    var watcher = new FileSystemWatcher(fileWatcherPipeline.WatchPath)
                    {
                        NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName,
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = false
                    };

                    watcher.Created += async (sender, e) =>
                    {
                        _logger.LogInformation("File created: {FilePath}, triggering pipeline", e.FullPath);
                        try
                        {
                            await _pipelineExecutor.ExecutePipelineAsync(fileWatcherPipeline.Pipeline, e.FullPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing file watcher pipeline for: {FilePath}", e.FullPath);
                        }
                    };

                    _fileWatchers.Add(watcher);
                    _logger.LogInformation("Started file watcher for: {WatchPath}", fileWatcherPipeline.WatchPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting file watcher for: {WatchPath}", fileWatcherPipeline.WatchPath);
                }
            }
        }

        public void StopFileWatchers()
        {
            foreach (var watcher in _fileWatchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _fileWatchers.Clear();
        }
    }
}