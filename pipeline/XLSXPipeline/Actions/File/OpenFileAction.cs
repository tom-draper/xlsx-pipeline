using System.Diagnostics;

namespace XLSXPipeline.Actions.File;

public class OpenFileAction : ActionBase
{
    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);
            LaunchFile(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open file: {ex.Message}", ex);
        }
    }

    private static void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Source file path cannot be null or empty");

        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Source file not found: {filePath}");
    }

    private static void LaunchFile(string filePath)
    {
        var process = CreateProcess(filePath);

        if (!process.Start())
            throw new InvalidOperationException($"Failed to start process for file: {filePath}");
    }

    private static Process CreateProcess(string filePath)
    {
        return new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            }
        };
    }
}