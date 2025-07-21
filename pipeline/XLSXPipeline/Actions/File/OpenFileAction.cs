using System.Diagnostics;

namespace XLSXPipeline.Actions.File;

public class OpenFileAction : ActionBase
{
    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Source file path cannot be null or empty");

            // Validate source file exists
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"Source file not found: {filePath}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // Launch with default app
                }
            };

            if (!process.Start())
                throw new InvalidOperationException($"Failed to start process for file: {filePath}");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(new InvalidOperationException(
                $"Failed to open file: {ex.Message}", ex));
        }
    }
}
