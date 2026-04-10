namespace XLSXPipeline.Services;

public interface IPipelineRegistry
{
    void Register(IReadOnlyList<Models.Pipeline> pipelines);
    Models.Pipeline? Find(string pipelineName);
}

public class PipelineRegistry : IPipelineRegistry
{
    private readonly Dictionary<string, Models.Pipeline> _pipelines = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IReadOnlyList<Models.Pipeline> pipelines)
    {
        _pipelines.Clear();
        foreach (var p in pipelines)
            _pipelines[p.PipelineName] = p;
    }

    public Models.Pipeline? Find(string pipelineName)
        => _pipelines.TryGetValue(pipelineName, out var p) ? p : null;
}
