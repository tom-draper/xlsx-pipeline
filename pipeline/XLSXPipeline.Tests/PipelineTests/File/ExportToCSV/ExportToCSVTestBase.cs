using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.ExportToCSV;

public abstract class ExportToCSVTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ExportToCSVAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "ExportToCSV"), defaultPipelineName)
{
    protected string GetOutputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var action = GetFirstAction(pipelineName);

        var inputPath = GetInputPath(pipelineName);
        var outputPath = Actions.Helpers.DetermineOutputPath(
            inputPath,
            "csv",
            action.OutputPath,
            action.FileName);

        return outputPath;
    }

    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);
        var outputPath = GetOutputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);
            AddTempFile(outputPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyFileIntegrity(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var outputPath = GetOutputPath(pipelineName);

        if (!System.IO.File.Exists(outputPath))
            throw new FileNotFoundException($"Output file should have been created for pipeline '{pipelineName}'.");

        var outputFileInfo = new FileInfo(outputPath);
        if (outputFileInfo.Length == 0)
            throw new InvalidOperationException($"Output file should not be empty for pipeline '{pipelineName}'.");
    }
}