namespace XLSXPipeline.Actions.File
{
    public class MoveFileAction : ActionBase
    {
        public required string DestinationPath { get; set; }
        public string? FilePath { get; set; }

        /// <summary>
        /// Whether to overwrite the destination file if it already exists
        /// </summary>
        public bool OverwriteIfExists { get; set; } = false;

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
                if (System.IO.Path.IsPathRooted(DestinationPath) &&
                    (System.IO.Path.HasExtension(DestinationPath) ||
                     DestinationPath.Contains(System.IO.Path.DirectorySeparatorChar) ||
                     DestinationPath.Contains(System.IO.Path.AltDirectorySeparatorChar)))
                {
                    // Treat DestinationPath as a full file path
                    destinationFilePath = DestinationPath;
                }
                else
                {
                    // Treat DestinationPath as a directory and combine with source filename
                    var fileName = System.IO.Path.GetFileName(sourceFilePath);
                    destinationFilePath = System.IO.Path.Combine(DestinationPath, fileName);
                }

                // Ensure destination directory exists
                var destinationDirectory = System.IO.Path.GetDirectoryName(destinationFilePath);
                if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                {
                    if (CreateDirectories)
                        Directory.CreateDirectory(destinationDirectory);
                    else
                        throw new DirectoryNotFoundException($"Destination directory does not exist: {destinationDirectory}");
                }

                // Check if destination file already exists and handle accordingly
                if (System.IO.File.Exists(destinationFilePath))
                {
                    if (!OverwriteIfExists)
                        throw new InvalidOperationException($"Destination file already exists and OverwriteIfExists is false: {destinationFilePath}");

                    // Delete the existing file if we're overwriting
                    System.IO.File.Delete(destinationFilePath);
                }

                // Perform the move operation
                System.IO.File.Move(sourceFilePath, destinationFilePath);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(new InvalidOperationException(
                    $"Failed to move file from '{FilePath ?? filePath}' to '{DestinationPath}': {ex.Message}", ex));
            }
        }
    }
}