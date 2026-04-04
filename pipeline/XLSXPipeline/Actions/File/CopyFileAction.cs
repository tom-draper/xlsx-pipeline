using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.File;

public class CopyFileAction : ActionBase
{
    [JsonPropertyName("destinationPath")]
    public PlaceholderString? DestinationPath { get; set; }

    [JsonPropertyName("fileName")]
    public PlaceholderString? FileName { get; set; }

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

            var destinationFilePath = Helpers.DetermineDestinationFilePath(filePath, DestinationPath!, AppendSourceExtension, FileName);
            Helpers.EnsureDirectory(destinationFilePath, CreateDirectories);
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
