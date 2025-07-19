namespace XLSXPipeline.Actions.File
{
    public class RenameFileAction : ActionBase
    {
        public required string NewName { get; set; }
        public string? FilePath { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(NewName))
                    throw new ArgumentException("NewName cannot be null or empty", nameof(NewName));

                // Use the Path property if provided, otherwise use the filePath argument
                var sourceFilePath = !string.IsNullOrEmpty(FilePath) ? FilePath : filePath;
                if (string.IsNullOrWhiteSpace(sourceFilePath))
                    throw new ArgumentException("Source file path cannot be null or empty");

                // Validate source file exists
                if (!System.IO.File.Exists(sourceFilePath))
                    throw new FileNotFoundException($"Source file not found: {sourceFilePath}");

                string destinationFilePath;

                // Check if NewName appears to be a full path (contains directory separators or has a drive letter)
                if (Path.IsPathRooted(NewName) ||
                    NewName.Contains(Path.DirectorySeparatorChar) ||
                    NewName.Contains(Path.AltDirectorySeparatorChar))
                {
                    // Treat NewName as a full file path
                    destinationFilePath = NewName;

                    // If the full path doesn't have an extension, add the original file's extension
                    if (string.IsNullOrEmpty(Path.GetExtension(destinationFilePath)))
                    {
                        var originalExtension = Path.GetExtension(sourceFilePath);
                        destinationFilePath = Path.ChangeExtension(destinationFilePath, originalExtension);
                    }
                }
                else
                {
                    // Treat NewName as just a filename
                    var sourceDirectory = Path.GetDirectoryName(sourceFilePath);
                    if (string.IsNullOrEmpty(sourceDirectory))
                        sourceDirectory = Directory.GetCurrentDirectory();

                    // If NewName doesn't have an extension, add the original file's extension
                    var newNameWithExtension = NewName;
                    if (string.IsNullOrEmpty(Path.GetExtension(NewName)))
                    {
                        var originalExtension = Path.GetExtension(sourceFilePath);
                        newNameWithExtension = Path.ChangeExtension(NewName, originalExtension);
                    }

                    destinationFilePath = Path.Combine(sourceDirectory, newNameWithExtension);
                }

                // Ensure destination directory exists
                var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
                if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);

                // Check if destination file already exists
                if (System.IO.File.Exists(destinationFilePath))
                    throw new InvalidOperationException($"Destination file already exists: {destinationFilePath}");

                // Perform the rename/move operation
                System.IO.File.Move(sourceFilePath, destinationFilePath);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(new InvalidOperationException(
                    $"Failed to rename file to '{NewName}': {ex.Message}", ex));
            }
        }
    }
}