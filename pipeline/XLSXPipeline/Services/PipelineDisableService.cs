using System.Text.Json;

namespace XLSXPipeline.Services;

public interface IPipelineDisableService
{
    Task MarkPipelineAsDisabled(string pipelineFilePath);
    Task<bool> IsPipelineDisabledAsync(string pipelineFilePath);
}

public class PipelineDisableService(ILogger<PipelineDisableService> logger) : IPipelineDisableService
{
    private readonly ILogger<PipelineDisableService> _logger = logger;
    private readonly SemaphoreSlim _fileSemaphore = new(1, 1);

    public async Task<bool> IsPipelineDisabledAsync(string pipelineFilePath)
    {
        await _fileSemaphore.WaitAsync();
        try
        {
            if (!File.Exists(pipelineFilePath))
                return false;

            var json = await File.ReadAllTextAsync(pipelineFilePath);
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("disabled", out var disabledElement))
                return disabledElement.GetBoolean();

            return false; // Default to not disabled if property doesn't exist
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking disabled status for pipeline: {FilePath}", pipelineFilePath);
            return false; // Default to not disabled on error
        }
        finally
        {
            _fileSemaphore.Release();
        }
    }

    public async Task MarkPipelineAsDisabled(string pipelineFilePath)
    {
        await _fileSemaphore.WaitAsync();
        try
        {
            if (!File.Exists(pipelineFilePath))
            {
                _logger.LogWarning("Cannot mark pipeline as disabled - file not found: {FilePath}", pipelineFilePath);
                return;
            }

            var json = await File.ReadAllTextAsync(pipelineFilePath);
            // The original JsonDocument should also be in a using block
            using var document = JsonDocument.Parse(json);

            var options = new JsonWriterOptions { Indented = true };
            await using var stream = new MemoryStream();
            await using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();

                // 1. Copy all existing properties except for "disabled"
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (!property.Name.Equals("disabled", StringComparison.OrdinalIgnoreCase))
                        property.WriteTo(writer);
                }

                // 2. Unconditionally write the "disabled" property as true.
                // This handles both adding it if it's new and overwriting it if it exists.
                writer.WriteBoolean("disabled", true);

                writer.WriteEndObject();
            } // The writer is disposed and flushed here automatically

            // 3. Reset stream position and write to file
            stream.Position = 0;
            using var fileStream = new FileStream(pipelineFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking pipeline as disabled: {FilePath}", pipelineFilePath);
        }
        finally
        {
            _fileSemaphore.Release();
        }
    }
}