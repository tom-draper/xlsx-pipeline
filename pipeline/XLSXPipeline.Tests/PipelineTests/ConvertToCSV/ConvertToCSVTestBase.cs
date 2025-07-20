using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.ConvertToCSV;

public abstract class ConvertToCSVTestBase : SpecializedPipelineTestBase<ConvertToCSVAction>
{
    protected ConvertToCSVTestBase(string? defaultPipelineName = null)
        : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "ConvertToCSV"), defaultPipelineName)
    {
    }

    protected string GetOutputPath(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var action = GetFirstAction(pipelineName);

        var outputPath = action.OutputPath ?? throw new InvalidOperationException($"No output path found in ConvertToCSVAction for pipeline '{pipelineName}'");

        if (!outputPath.EndsWith(".csv"))
            outputPath += ".csv";

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

            Console.WriteLine("PATH");
            Console.WriteLine(outputPath);
        }
        catch (Exception ex)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyFileIntegrity(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var outputPath = GetOutputPath(pipelineName);

        if (!File.Exists(outputPath))
            throw new FileNotFoundException($"Output file should have been created for pipeline '{pipelineName}'.");

        var outputFileInfo = new FileInfo(outputPath);
        if (outputFileInfo.Length == 0)
            throw new InvalidOperationException($"Output file should not be empty for pipeline '{pipelineName}'.");
    }
}