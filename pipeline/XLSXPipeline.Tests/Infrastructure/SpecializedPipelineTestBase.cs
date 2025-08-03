using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

public abstract class SpecializedPipelineTestBase<TAction> : PipelineTestBase
    where TAction : class
{
    protected readonly string DefaultPipelineName;

    protected SpecializedPipelineTestBase(string testDirectory, string? defaultPipelineName = null)
        : base(testDirectory)
    {
        DefaultPipelineName = defaultPipelineName ?? GetFirstPipelineNameWithAction();
    }

    protected override void UpdatePipelinePaths(Pipeline pipeline)
    {
        UpdatePipelinePathsForAction<TAction>(pipeline);
    }

    protected new string GetInputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        return base.GetInputPath(pipelineName);
    }

    protected IEnumerable<string> GetPipelineNamesWithAction()
    {
        return Pipelines
            .Where(kvp => kvp.Value.Actions.OfType<TAction>().Any())
            .Select(kvp => kvp.Key);
    }

    protected TAction GetFirstAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        return pipeline.Actions.OfType<TAction>().FirstOrDefault()
            ?? throw new InvalidOperationException($"No {typeof(TAction).Name} found in pipeline '{pipelineName}'");
    }

    protected TAction GetLastAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        return pipeline.Actions.OfType<TAction>().LastOrDefault()
            ?? throw new InvalidOperationException($"No {typeof(TAction).Name} found in pipeline '{pipelineName}'");
    }

    protected IEnumerable<TAction> GetAllActions(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        return pipeline.Actions.OfType<TAction>();
    }

    private string GetFirstPipelineNameWithAction()
    {
        var pipelineName = GetPipelineNamesWithAction().FirstOrDefault();
        if (pipelineName == null)
            throw new InvalidOperationException($"No pipelines with {typeof(TAction).Name} actions found. Available pipelines: " +
                                              string.Join(", ", GetPipelineNames()));
        return pipelineName;
    }

    protected async Task ExecuteAllPipelinesAsync()
    {
        var pipelineNames = GetPipelineNamesWithAction();

        foreach (var pipelineName in pipelineNames)
            await ExecutePipelineTestAsync(pipelineName);
    }

    protected abstract Task ExecutePipelineTestAsync(string? pipelineName = null);
}

// Result classes for better error handling
public class PipelineExecutionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static PipelineExecutionResult CreateSuccess() => new() { Success = true };

    public static PipelineExecutionResult CreateFailure(string errorMessage, Exception? exception = null) =>
        new() { Success = false, ErrorMessage = errorMessage, Exception = exception };
}