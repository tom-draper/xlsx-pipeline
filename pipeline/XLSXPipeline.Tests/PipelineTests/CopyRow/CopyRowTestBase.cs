using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.CopyRow;

public abstract class CopyColumnTestBase : PipelineTestBase
{
    protected readonly string DefaultPipelineName;

    protected CopyColumnTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "CopyRow"))
    {
        DefaultPipelineName = defaultPipelineName ?? GetFirstCopyRowPipelineName();
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
    /// Executes the CopyRow pipeline test
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline to execute. If null, uses the default pipeline.</param>
    /// <returns>True if the output file was created successfully</returns>
    protected async Task<bool> ExecuteCopyRowTestAsync(string? pipelineName = null)
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
            ExcelTestHelpers.VerifyRowsAreIdentical(worksheet, 1, 2);
            return true;
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    /// <summary>
    /// Executes all CopyRow pipelines
    /// </summary>
    /// <returns>Dictionary of pipeline names and their execution results</returns>
    protected async Task<Dictionary<string, bool>> ExecuteAllCopyRowTestsAsync()
    {
        var results = new Dictionary<string, bool>();
        var pipelines = GetCopyRowPipelineNames();

        foreach (var pipelineName in pipelines)
        {
            results[pipelineName] = await ExecuteCopyRowTestAsync(pipelineName);
        }

        return results;
    }

    /// <summary>
    /// Verifies the renamed file for a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    protected void VerifyCopyRow(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = base.GetInputPath(pipelineName);
    }

    /// <summary>
    /// Gets all pipeline names that contain CopyRow actions
    /// </summary>
    /// <returns>Collection of pipeline names with CopyRow actions</returns>
    protected IEnumerable<string> GetCopyRowPipelineNames()
    {
        return Pipelines
            .Where(kvp => kvp.Value.Actions.OfType<CopyRowAction>().Any())
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Gets the first available CopyRow pipeline name
    /// </summary>
    /// <returns>The name of the first CopyRow pipeline</returns>
    /// <exception cref="InvalidOperationException">Thrown when no CopyRow pipelines are found</exception>
    private string GetFirstCopyRowPipelineName()
    {
        var renameFilePipelineName = GetCopyRowPipelineNames().FirstOrDefault();

        if (renameFilePipelineName == null)
        {
            throw new InvalidOperationException("No pipelines with CopyRow actions found. Available pipelines: " +
                                              string.Join(", ", GetPipelineNames()));
        }

        return renameFilePipelineName;
    }

    /// <summary>
    /// Gets the CopyRow action from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>The CopyRow action</returns>
    protected CopyRowAction GetCopyRowAction(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions
            .OfType<CopyRowAction>()
            .FirstOrDefault() ?? throw new InvalidOperationException($"No CopyRowAction found in pipeline '{pipelineName}'");
    }

    /// <summary>
    /// Gets all CopyRow actions from a specific pipeline
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline. If null, uses the default pipeline.</param>
    /// <returns>Collection of CopyRow actions</returns>
    protected IEnumerable<CopyRowAction> GetCopyRowActions(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);

        return pipeline.Actions.OfType<CopyRowAction>();
    }
}