namespace XLSXPipeline.Actions.Time;

public class WaitAction : ActionBase
{
    public int TimeInSeconds { get; set; }

    protected override async Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateTimeInSeconds();
            await WaitAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to wait for {TimeInSeconds} seconds: {ex.Message}", ex);
        }
    }

    private void ValidateTimeInSeconds()
    {
        if (TimeInSeconds < 0)
            throw new ArgumentException("TimeInSeconds cannot be negative", nameof(TimeInSeconds));
    }

    private async Task WaitAsync()
    {
        var delay = TimeSpan.FromSeconds(TimeInSeconds);
        await Task.Delay(delay);
    }
}