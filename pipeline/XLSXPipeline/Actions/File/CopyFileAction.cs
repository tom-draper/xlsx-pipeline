namespace XLSXPipeline.Actions.File
{
    public class CopyFileAction : ActionBase
    {
        public required string DestinationPath { get; set; }
        public string? FilePath { get; set; }

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

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(DestinationPath))
                    throw new ArgumentException("DestinationPath cannot be null or empty", nameof(DestinationPath));

                // Use the Path property if provided, otherwise use the filePath argument
                var sourceFilePath = !string.IsNullOrEmpty(FilePath) ? FilePath : filePath;

                if (string.IsNullOrWhiteSpace(sourceFilePath))
                    throw new ArgumentException("Source file path cannot be null or empty");

                // Validate source file exists
                if (!System.IO.File.Exists(sourceFilePath))
                    throw new FileNotFoundException($"Source file not found: {sourceFilePath}");

                string destinationFilePath;

                // Check if DestinationPath appears to be a full file path
                if (Path.IsPathRooted(DestinationPath) &&
                    (Path.HasExtension(DestinationPath) ||
                     DestinationPath.Contains(Path.DirectorySeparatorChar) ||
                     DestinationPath.Contains(Path.AltDirectorySeparatorChar)) || DestinationPath.EndsWith(".xlsx"))
                {
                    // Treat DestinationPath as a full file path
                    destinationFilePath = DestinationPath;
                }
                else
                {
                    // Treat DestinationPath as a directory and combine with source filename
                    var fileName = Path.GetFileName(sourceFilePath);
                    destinationFilePath = Path.Combine(DestinationPath, fileName);
                }

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
                    {
                        // Find next available file name
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

                        destinationFilePath = newFilePath;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Destination file already exists and OverwriteIfExists is false: {destinationFilePath}");
                    }
                }

                // Perform the copy operation
                System.IO.File.Copy(sourceFilePath, destinationFilePath, OverwriteIfExists);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(new InvalidOperationException(
                    $"Failed to copy file from '{FilePath ?? filePath}' to '{DestinationPath}': {ex.Message}", ex));
            }
        }
    }
}