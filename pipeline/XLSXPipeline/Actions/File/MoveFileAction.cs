namespace XLSXPipeline.Actions.File;

public class MoveFileAction : ActionBase
{
    public required string DestinationPath { get; set; }

    /// <summary>
    /// Whether to overwrite the destination file if it already exists
    /// </summary>
    public bool OverwriteIfExists { get; set; } = false;

    /// <summary>
    /// Whether to create destination directories if they don't exist
    /// </summary>
    public bool CreateDirectories { get; set; } = true;

    /// <summary>
    /// Whether to automatically append the source file extension if destination doesn't have one
    /// </summary>
    public bool AppendSourceExtension { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);

            var destinationFilePath = DetermineDestinationFilePath(filePath);
            EnsureDestinationDirectory(destinationFilePath);
            HandleExistingFile(destinationFilePath);

            System.IO.File.Move(filePath, destinationFilePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to move file from '{filePath}' to '{DestinationPath}': {ex.Message}", ex);
        }
    }

    private void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(DestinationPath))
            throw new ArgumentException("DestinationPath cannot be null or empty", nameof(DestinationPath));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Source file path cannot be null or empty");

        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Source file not found: {filePath}");
    }

    private string DetermineDestinationFilePath(string sourceFilePath)
    {
        if (IsDestinationDirectory())
            return CombineWithSourceFileName(sourceFilePath);

        if (HasFileExtension())
            return DestinationPath;

        if (IsExistingDirectory())
            return CombineWithSourceFileName(sourceFilePath);

        if (IsFileInExistingDirectory())
            return AppendExtensionIfNeeded(sourceFilePath, DestinationPath);

        return DetermineByPathStructure(sourceFilePath);
    }

    private bool IsDestinationDirectory()
    {
        return DestinationPath.EndsWith(Path.DirectorySeparatorChar) ||
               DestinationPath.EndsWith(Path.AltDirectorySeparatorChar);
    }

    private bool HasFileExtension()
    {
        return Path.HasExtension(DestinationPath);
    }

    private bool IsExistingDirectory()
    {
        return Directory.Exists(DestinationPath);
    }

    private bool IsFileInExistingDirectory()
    {
        var parentDir = Path.GetDirectoryName(DestinationPath);
        return !string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir);
    }

    private string CombineWithSourceFileName(string sourceFilePath)
    {
        var fileName = Path.GetFileName(sourceFilePath);
        return Path.Combine(DestinationPath, fileName);
    }

    private string AppendExtensionIfNeeded(string sourceFilePath, string destinationPath)
    {
        if (!AppendSourceExtension)
            return destinationPath;

        var sourceExtension = Path.GetExtension(sourceFilePath);
        return destinationPath + sourceExtension;
    }

    private string DetermineByPathStructure(string sourceFilePath)
    {
        if (LooksLikeFilePath())
            return AppendExtensionIfNeeded(sourceFilePath, DestinationPath);

        return CombineWithSourceFileName(sourceFilePath);
    }

    private bool LooksLikeFilePath()
    {
        return Path.IsPathRooted(DestinationPath) &&
               (DestinationPath.Contains(Path.DirectorySeparatorChar) ||
                DestinationPath.Contains(Path.AltDirectorySeparatorChar));
    }

    private void EnsureDestinationDirectory(string destinationFilePath)
    {
        var destinationDirectory = Path.GetDirectoryName(destinationFilePath);

        if (string.IsNullOrEmpty(destinationDirectory) || Directory.Exists(destinationDirectory))
            return;

        if (CreateDirectories)
            Directory.CreateDirectory(destinationDirectory);
        else
            throw new DirectoryNotFoundException($"Destination directory does not exist: {destinationDirectory}");
    }

    private void HandleExistingFile(string destinationFilePath)
    {
        if (!System.IO.File.Exists(destinationFilePath))
            return;

        if (!OverwriteIfExists)
            throw new InvalidOperationException(
                $"Destination file already exists and OverwriteIfExists is false: {destinationFilePath}");

        System.IO.File.Delete(destinationFilePath);
    }
}