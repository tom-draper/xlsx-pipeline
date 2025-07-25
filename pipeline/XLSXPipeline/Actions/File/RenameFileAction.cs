namespace XLSXPipeline.Actions.File;

public class RenameFileAction : ActionBase
{
    public required string NewName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);

            var destinationFilePath = DetermineDestinationPath(filePath);
            EnsureDestinationDirectory(destinationFilePath);
            ValidateDestinationNotExists(destinationFilePath);

            System.IO.File.Move(filePath, destinationFilePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to rename file to '{NewName}': {ex.Message}", ex);
        }
    }

    private void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(NewName))
            throw new ArgumentException("NewName cannot be null or empty", nameof(NewName));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Source file path cannot be null or empty");

        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Source file not found: {filePath}");
    }

    private string DetermineDestinationPath(string filePath)
    {
        if (IsFullPath())
            return BuildFullPathDestination(filePath);

        return BuildFilenameDestination(filePath);
    }

    private bool IsFullPath()
    {
        return Path.IsPathRooted(NewName) ||
               NewName.Contains(Path.DirectorySeparatorChar) ||
               NewName.Contains(Path.AltDirectorySeparatorChar);
    }

    private string BuildFullPathDestination(string filePath)
    {
        var destinationPath = NewName;

        if (string.IsNullOrEmpty(Path.GetExtension(destinationPath)))
        {
            var originalExtension = Path.GetExtension(filePath);
            destinationPath = Path.ChangeExtension(destinationPath, originalExtension);
        }

        return destinationPath;
    }

    private string BuildFilenameDestination(string filePath)
    {
        var sourceDirectory = GetSourceDirectory(filePath);
        var newNameWithExtension = EnsureExtension(filePath, NewName);

        return Path.Combine(sourceDirectory, newNameWithExtension);
    }

    private static string GetSourceDirectory(string filePath)
    {
        var sourceDirectory = Path.GetDirectoryName(filePath);
        return string.IsNullOrEmpty(sourceDirectory) ? Directory.GetCurrentDirectory() : sourceDirectory;
    }

    private static string EnsureExtension(string filePath, string fileName)
    {
        if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
            return fileName;

        var originalExtension = Path.GetExtension(filePath);
        return Path.ChangeExtension(fileName, originalExtension);
    }

    private static void EnsureDestinationDirectory(string destinationFilePath)
    {
        var destinationDirectory = Path.GetDirectoryName(destinationFilePath);

        if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);
    }

    private static void ValidateDestinationNotExists(string destinationFilePath)
    {
        if (System.IO.File.Exists(destinationFilePath))
            throw new InvalidOperationException($"Destination file already exists: {destinationFilePath}");
    }
}