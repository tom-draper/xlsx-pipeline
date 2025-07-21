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
            // Validate inputs
            if (string.IsNullOrWhiteSpace(DestinationPath))
                throw new ArgumentException("DestinationPath cannot be null or empty", nameof(DestinationPath));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Source file path cannot be null or empty");

            // Validate source file exists
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"Source file not found: {filePath}");

            string destinationFilePath = DetermineDestinationFilePath(filePath, DestinationPath);

            // Ensure destination directory exists
            var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                if (CreateDirectories)
                    Directory.CreateDirectory(destinationDirectory);
                else
                    throw new DirectoryNotFoundException($"Destination directory does not exist: {destinationDirectory}");
            }

            // Check if destination file already exists and handle accordingly
            if (System.IO.File.Exists(destinationFilePath) && !OverwriteIfExists)
            {
                if (AutoRenameIfExists)
                    destinationFilePath = GetAvailableFileName(destinationFilePath);
                else
                    throw new InvalidOperationException($"Destination file already exists and OverwriteIfExists is false: {destinationFilePath}");
            }

            // Perform the copy operation
            System.IO.File.Copy(filePath, destinationFilePath, OverwriteIfExists);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(new InvalidOperationException(
                $"Failed to copy file from '{FilePath ?? filePath}' to '{DestinationPath}': {ex.Message}", ex));
        }
    }

    private string DetermineDestinationFilePath(string sourceFilePath, string destinationPath)
    {
        // Check if destination is clearly a directory (ends with directory separator)
        if (destinationPath.EndsWith(Path.DirectorySeparatorChar) ||
            destinationPath.EndsWith(Path.AltDirectorySeparatorChar))
        {
            var fileName = Path.GetFileName(sourceFilePath);
            return Path.Combine(destinationPath, fileName);
        }

        // Check if destination has an extension
        if (Path.HasExtension(destinationPath))
        {
            // Treat as a full file path
            return destinationPath;
        }

        // Check if destination appears to be an existing directory
        if (Directory.Exists(destinationPath))
        {
            var fileName = Path.GetFileName(sourceFilePath);
            return Path.Combine(destinationPath, fileName);
        }

        // Check if the parent directory exists, suggesting this is meant to be a file
        var parentDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
        {
            // Destination path appears to be a file without extension
            if (AppendSourceExtension)
            {
                var sourceExtension = Path.GetExtension(sourceFilePath);
                return destinationPath + sourceExtension;
            }
            return destinationPath;
        }

        // Default behavior - check if path looks like a file path structure
        if (Path.IsPathRooted(destinationPath) &&
            (destinationPath.Contains(Path.DirectorySeparatorChar) ||
             destinationPath.Contains(Path.AltDirectorySeparatorChar)))
        {
            // Looks like a file path - append extension if needed
            if (AppendSourceExtension)
            {
                var sourceExtension = Path.GetExtension(sourceFilePath);
                return destinationPath + sourceExtension;
            }
            return destinationPath;
        }
        else
        {
            // Treat as directory and combine with source filename
            var fileName = Path.GetFileName(sourceFilePath);
            return Path.Combine(destinationPath, fileName);
        }
    }

    private string GetAvailableFileName(string destinationFilePath)
    {
        var directory = Path.GetDirectoryName(destinationFilePath)!;
        var originalFileName = Path.GetFileNameWithoutExtension(destinationFilePath);
        var extension = Path.GetExtension(destinationFilePath);

        string baseCopyName = $"{originalFileName} - Copy";
        string newFilePath = Path.Combine(directory, $"{baseCopyName}{extension}");
        int copyIndex = 2;

        while (System.IO.File.Exists(newFilePath))
        {
            newFilePath = Path.Combine(directory, $"{baseCopyName} ({copyIndex}){extension}");
            copyIndex++;
        }

        return newFilePath;
    }
}