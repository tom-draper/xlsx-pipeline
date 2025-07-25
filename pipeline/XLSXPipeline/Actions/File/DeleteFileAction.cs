namespace XLSXPipeline.Actions.File;

public class DeleteFileAction : ActionBase
{
    /// <summary>
    /// Whether to ignore if the file doesn't exist (no error thrown)
    /// </summary>
    public bool IgnoreIfNotExists { get; set; } = true;

    /// <summary>
    /// Whether to remove read-only attribute before deletion if present
    /// </summary>
    public bool RemoveReadOnlyAttribute { get; set; } = false;

    /// <summary>
    /// Optional backup directory path. If specified, file will be moved here before deletion.
    /// </summary>
    public string? BackupDirectory { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);

            if (!System.IO.File.Exists(filePath))
            {
                if (IgnoreIfNotExists)
                    return Task.CompletedTask;

                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Create backup if specified
            if (!string.IsNullOrWhiteSpace(BackupDirectory))
                CreateBackup(filePath);

            // Handle read-only files
            if (RemoveReadOnlyAttribute)
            {
                var attributes = System.IO.File.GetAttributes(filePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    System.IO.File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
            }

            System.IO.File.Delete(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to delete file '{filePath}': {ex.Message}", ex);
        }
    }

    private static void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
    }

    private void CreateBackup(string filePath)
    {
        if (string.IsNullOrWhiteSpace(BackupDirectory))
            return;

        if (!Directory.Exists(BackupDirectory))
            Directory.CreateDirectory(BackupDirectory);

        var fileName = Path.GetFileName(filePath);
        var backupPath = Path.Combine(BackupDirectory, fileName);

        // Handle existing backup files
        int backupIndex = 1;
        while (System.IO.File.Exists(backupPath))
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var indexedName = $"{nameWithoutExtension}_backup_{backupIndex}{extension}";
            backupPath = Path.Combine(BackupDirectory, indexedName);
            backupIndex++;
        }

        System.IO.File.Copy(filePath, backupPath);
    }
}