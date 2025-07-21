
namespace XLSXPipeline.Actions.Time;

public class WaitAction : ActionBase
{
    public int TimeInSeconds { get; set; }

    public override Task ExecuteAsync(string filePath)
    {
        Thread.Sleep(TimeInSeconds * 1000);
        return Task.CompletedTask;
    }
}