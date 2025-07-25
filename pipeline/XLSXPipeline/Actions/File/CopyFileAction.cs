namespace XLSXPipeline.Actions.File;

public class CopyFileAction : ActionBase
{
    public required string DestinationPath { get; set; }

    /// <summary>
    /// Whether to overwrite the destination file if it already exists
    /// </summary>
    public bool OverwriteIfExists { get; set; } = true;

    /// <summary>
    /// Whether to automatically rename the file if it already exists (Windows-style - Copy, - Copy (2), etc.)
    /// </summary>
    public bool AutoRenameIfExists { get; set; } = false;

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
            destinationFilePath = HandleExistingFile(destinationFilePath);

            System.IO.File.Copy(filePath, destinationFilePath, OverwriteIfExists);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to copy file from '{filePath}' to '{DestinationPath}': {ex.Message}", ex);
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

        if (IsFileInExistingDirectory(sourceFilePath))
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

    private bool IsFileInExistingDirectory(string sourceFilePath)
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

    private string HandleExistingFile(string destinationFilePath)
    {
        if (!System.IO.File.Exists(destinationFilePath) || OverwriteIfExists)
            return destinationFilePath;

        if (AutoRenameIfExists)
            return GetAvailableFileName(destinationFilePath);

        throw new InvalidOperationException(
            $"Destination file already exists and OverwriteIfExists is false: {destinationFilePath}");
    }

    private static string GetAvailableFileName(string destinationFilePath)
    {
        var directory = Path.GetDirectoryName(destinationFilePath)!;
        var originalFileName = Path.GetFileNameWithoutExtension(destinationFilePath);
        var extension = Path.GetExtension(destinationFilePath);

        var baseCopyName = $"{originalFileName} - Copy";
        var newFilePath = Path.Combine(directory, $"{baseCopyName}{extension}");
        int copyIndex = 2;

        while (System.IO.File.Exists(newFilePath))
        {
            newFilePath = Path.Combine(directory, $"{baseCopyName} ({copyIndex}){extension}");
            copyIndex++;
        }

        return newFilePath;
    }
}