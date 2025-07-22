using XLSXPipeline.Models;

namespace XLSXPipeline.Services;

public interface IFileWatcherService
{
    void StartFileWatchers(List<FileWatcherPipeline> fileWatcherPipelines);
    void StopFileWatchers();
}

public class FileWatcherService(ILogger<FileWatcherService> logger, IPipelineExecutor pipelineExecutor) : IFileWatcherService, IDisposable
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
                string directoryPath;
                string fileFilter;

                // Check if the path is a file or directory
                if (File.Exists(fileWatcherPipeline.WatchPath))
                {
                    // It's a file - watch its directory with specific filter
                    directoryPath = Path.GetDirectoryName(fileWatcherPipeline.WatchPath)!;
                    fileFilter = Path.GetFileName(fileWatcherPipeline.WatchPath);
                    _logger.LogInformation("Watching specific file: {FilePath}", fileWatcherPipeline.WatchPath);
                }
                else if (Directory.Exists(fileWatcherPipeline.WatchPath))
                {
                    // It's a directory - watch all files
                    directoryPath = fileWatcherPipeline.WatchPath;
                    fileFilter = "*.*";
                    _logger.LogInformation("Watching directory: {DirectoryPath}", fileWatcherPipeline.WatchPath);
                }
                else
                {
                    // Path doesn't exist - check if it looks like a file path
                    var directory = Path.GetDirectoryName(fileWatcherPipeline.WatchPath);
                    if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    {
                        // Directory exists but file doesn't - watch for the file to be created
                        directoryPath = directory;
                        fileFilter = Path.GetFileName(fileWatcherPipeline.WatchPath);
                        _logger.LogInformation("Watching for file creation: {FilePath}", fileWatcherPipeline.WatchPath);
                    }
                    else
                    {
                        _logger.LogWarning("Watch path and its directory do not exist: {WatchPath}", fileWatcherPipeline.WatchPath);
                        continue;
                    }
                }

                var watcher = new FileSystemWatcher(directoryPath)
                {
                    Filter = fileFilter,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = false
                };

                watcher.Created += (sender, e) =>
                {
                    _ = Task.Run(async () =>
                    {
                        _logger.LogInformation("File created: {FilePath}, triggering pipeline", e.FullPath);
                        await ExecutePipelineWithRetry(fileWatcherPipeline.Pipeline, e.FullPath);
                    });
                };

                watcher.Changed += (sender, e) =>
                {
                    _ = Task.Run(async () =>
                    {
                        _logger.LogInformation("File changed: {FilePath}, triggering pipeline", e.FullPath);
                        await ExecutePipelineWithRetry(fileWatcherPipeline.Pipeline, e.FullPath);
                    });
                };

                _fileWatchers.Add(watcher);
                _logger.LogInformation("Started file watcher for: {WatchPath} (Directory: {DirectoryPath}, Filter: {FileFilter})",
                    fileWatcherPipeline.WatchPath, directoryPath, fileFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting file watcher for: {WatchPath}", fileWatcherPipeline.WatchPath);
            }
        }
    }

    private async Task ExecutePipelineWithRetry(Pipeline pipeline, string filePath)
    {
        const int maxRetries = 3;
        const int retryDelayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Wait a bit to ensure file is fully written and not locked
                if (attempt > 1)
                {
                    await Task.Delay(retryDelayMs * attempt);
                }
                else
                {
                    await Task.Delay(500); // Initial delay
                }

                // Check if file is accessible
                if (IsFileReady(filePath))
                {
                    await _pipelineExecutor.ExecutePipelineAsync(pipeline, filePath);
                    return; // Success, exit retry loop
                }
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning("Pipeline execution attempt {Attempt} failed for {FilePath}: {Error}",
                    attempt, filePath, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing file watcher pipeline for: {FilePath} after {MaxRetries} attempts",
                    filePath, maxRetries);
                return;
            }
        }

        _logger.LogError("Failed to execute pipeline for {FilePath} after {MaxRetries} attempts - file may be locked",
            filePath, maxRetries);
    }

    private static bool IsFileReady(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public void StopFileWatchers()
    {
        foreach (var watcher in _fileWatchers)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing file watcher");
            }
        }
        _fileWatchers.Clear();
    }

    public void Dispose()
    {
        StopFileWatchers();
        GC.SuppressFinalize(this);
    }
}