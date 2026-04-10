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
    private readonly Dictionary<string, CancellationTokenSource> _pendingExecutions = [];
    private readonly object _pendingLock = new();

    public void StartFileWatchers(List<FileWatcherPipeline> fileWatcherPipelines)
    {
        foreach (var fileWatcherPipeline in fileWatcherPipelines)
        {
            foreach (var watchPath in fileWatcherPipeline.WatchPaths)
            {
                try
                {
                    string directoryPath;
                    string fileFilter;

                    // Check if the path is a file or directory
                    if (File.Exists(watchPath))
                    {
                        // It's a file - watch its directory with specific filter
                        directoryPath = Path.GetDirectoryName(watchPath)!;
                        fileFilter = Path.GetFileName(watchPath);
                        _logger.LogInformation("Watching specific file: {WatchPath} (Directory: {DirectoryPath}, Filter: {FileFilter})",
                            watchPath, directoryPath, fileFilter);
                    }
                    else if (Directory.Exists(watchPath))
                    {
                        // It's a directory - watch all files
                        directoryPath = watchPath;
                        fileFilter = "*.*";
                        _logger.LogInformation("Watching directory: {DirectoryPath}", watchPath);
                    }
                    else
                    {
                        // Path doesn't exist - check if it looks like a file path
                        var directory = Path.GetDirectoryName(watchPath);
                        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                        {
                            // Directory exists but file doesn't - watch for the file to be created
                            directoryPath = directory;
                            fileFilter = Path.GetFileName(watchPath);
                            _logger.LogInformation("Watching for file creation: {FilePath}", watchPath);
                        }
                        else
                        {
                            _logger.LogWarning("Watch path and its directory do not exist: {WatchPath}", watchPath);
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

                    watcher.Created += (sender, e) => ScheduleDebouncedExecution(fileWatcherPipeline.Pipeline, e.FullPath);
                    watcher.Changed += (sender, e) => ScheduleDebouncedExecution(fileWatcherPipeline.Pipeline, e.FullPath);

                    _fileWatchers.Add(watcher);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting file watcher for: {WatchPath}", watchPath);
                }
            }
        }
    }

    private void ScheduleDebouncedExecution(Pipeline pipeline, string filePath)
    {
        CancellationTokenSource cts;
        lock (_pendingLock)
        {
            if (_pendingExecutions.TryGetValue(filePath, out var existing))
                existing.Cancel();
            cts = new CancellationTokenSource();
            _pendingExecutions[filePath] = cts;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                // Debounce window: discard all but the last event within 500 ms
                await Task.Delay(500, cts.Token);
                lock (_pendingLock)
                    _pendingExecutions.Remove(filePath);

                _logger.LogInformation("File event triggered pipeline: {FilePath}", filePath);
                await ExecutePipelineWithRetry(pipeline, filePath);
            }
            catch (OperationCanceledException)
            {
                // A newer event arrived — this execution was superseded
            }
        });
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
                    await Task.Delay(retryDelayMs * attempt);
                else
                    await Task.Delay(500); // Initial delay

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
        lock (_pendingLock)
        {
            foreach (var cts in _pendingExecutions.Values)
                cts.Cancel();
            _pendingExecutions.Clear();
        }

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