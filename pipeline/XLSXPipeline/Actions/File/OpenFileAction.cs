using System.Diagnostics;

namespace XLSXPipeline.Actions.File;

public class OpenFileAction : ActionBase
{
    public string? FilePath { get; set; }

    public override Task ExecuteAsync(string filePath)
    {
        try
        {
            var targetFilePath = !string.IsNullOrEmpty(FilePath) ? FilePath : filePath;
            if (string.IsNullOrWhiteSpace(targetFilePath))
                throw new ArgumentException("File path cannot be null or empty");

            if (!System.IO.File.Exists(targetFilePath))
                throw new FileNotFoundException($"File not found: {targetFilePath}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = targetFilePath,
                    UseShellExecute = true // Launch with default app
                }
            };

            if (!process.Start())
                throw new InvalidOperationException($"Failed to start process for file: {targetFilePath}");


            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(new InvalidOperationException(
                $"Failed to open file: {ex.Message}", ex));
        }
    }
}
