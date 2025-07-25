namespace XLSXPipeline.Actions.File;

public class CreateFileAction : ActionBase
{
    public required string DestinationPath { get; set; }

    /// <summary>
    /// Content to write to the file. If null or empty, creates an empty file.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Whether to overwrite the file if it already exists
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
    /// Text encoding to use when writing content. Defaults to UTF-8.
    /// </summary>
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs();
            Helpers.EnsureDirectory(DestinationPath, CreateDirectories);
            var destinationFilePath = HandleExistingFile(DestinationPath);

            if (string.IsNullOrEmpty(Content))
                // Create empty file
                System.IO.File.Create(destinationFilePath).Dispose();
            else
                System.IO.File.WriteAllText(destinationFilePath, Content, Encoding);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create file at '{DestinationPath}': {ex.Message}", ex);
        }
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(DestinationPath))
            throw new ArgumentException("DestinationPath cannot be null or empty", nameof(DestinationPath));
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