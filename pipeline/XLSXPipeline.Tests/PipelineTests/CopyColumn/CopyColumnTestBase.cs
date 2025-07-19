using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.CopyColumn;

public abstract class CopyColumnTestBase : PipelineTestBase
{
    protected readonly string DefaultPipelineName;

    protected CopyColumnTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "CopyColumn"))
    {
        DefaultPipelineName = defaultPipelineName ?? GetFirstCopyColumnPipelineName();
    }

    /// <summary>
    /// Gets the input path for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The full input path for the pipeline</returns>
    protected new string GetInputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        return base.GetInputPath(pipelineName);
    }

    /// <summary>
    /// Executes the CopyColumn pipeline test
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline to execute. If null, uses the default pipeline.</param>
    /// <returns>True if the output file was created successfully</returns>
    protected async Task<bool> ExecuteCopyColumnTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = base.GetInputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);

            using var workbook = new XLWorkbook(inputPath);
            var worksheet = workbook.Worksheets.First();
            ExcelTestHelpers.VerifyColumnsAreIdentical(worksheet, 1, 2);
            return true;
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    /// <summary>
    /// Executes all CopyColumn pipelines
    /// </summary>
    /// <returns>Dictionary of pipeline names and their execution results</returns>
    protected async Task<Dictionary<string, bool>> ExecuteAllCopyColumnTestsAsync()
    {
        var results = new Dictionary<string, bool>();
        var pipelines = GetCopyColumnPipelineNames();

        foreach (var pipelineName in pipelines)
        {
            results[pipelineName] = await ExecuteCopyColumnTestAsync(pipelineName);
        }

        return results;
    }

    /// <summary>
    /// Verifies the renamed file for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    protected void VerifyCopyColumn(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = base.GetInputPath(pipelineName);
    }

    /// <summary>
    /// Gets all pipeline names that contain CopyColumn actions
    /// </summary>
    /// <returns>Collection of pipeline names with CopyColumn actions</returns>
    protected IEnumerable<string> GetCopyColumnPipelineNames()
    {
        return Pipelines
            .Where(kvp => kvp.Value.Actions.OfType<CopyColumnAction>().Any())
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Gets the first available CopyColumn pipeline name
    /// </summary>
    /// <returns>The name of the first CopyColumn pipeline</returns>
    /// <exception cref="InvalidOperationException">Thrown when no CopyColumn pipelines are found</exception>
    private string GetFirstCopyColumnPipelineName()
    {
        var renameFilePipelineName = GetCopyColumnPipelineNames().FirstOrDefault();

        if (renameFilePipelineName == null)
        {
            throw new InvalidOperationException("No pipelines with CopyColumn actions found. Available pipelines: " +
                                              string.Join(", ", GetPipelineNames()));
        }

        return renameFilePipelineName;
    }

    /// <summary>
    /// Gets the CopyColumn action from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The CopyColumn action</returns>
    protected CopyColumnAction GetCopyColumnAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions
            .OfType<CopyColumnAction>()
            .FirstOrDefault() ?? throw new InvalidOperationException($"No CopyColumnAction found in pipeline '{pipelineName}'");
    }

    /// <summary>
    /// Gets all CopyColumn actions from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>Collection of CopyColumn actions</returns>
    protected IEnumerable<CopyColumnAction> GetCopyColumnActions(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions.OfType<CopyColumnAction>();
    }
}