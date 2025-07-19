using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.ConvertToCSV;

public abstract class ConvertToCSVTestBase : PipelineTestBase
{
    protected readonly string OutputPath;

    protected ConvertToCSVTestBase() : base(@"..\..\..\PipelineTests\ConvertToCSV")
    {
        OutputPath = Path.GetFullPath(GetOutputPath());
    }

    protected override void UpdatePipelinePaths()
    {
        UpdatePipelinePathsForAction<ConvertToCSVAction>();
    }

    private string GetOutputPath()
    {
        return Pipeline.Actions
            .OfType<ConvertToCSVAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.OutputPath))?
            .OutputPath ?? throw new InvalidOperationException("No ConvertToCSVAction with output path found");
    }

    protected async Task<bool> ExecuteConvertToCSVAsync()
    {
        try
        {
            ExcelTestHelpers.CreateTestFile(InputPath);
            AddTempFile(InputPath);
            AddTempFile(OutputPath);

            var pipelineExecutor = await GetPipelineExecutorAsync();
            await pipelineExecutor.ExecutePipelineAsync(Pipeline, InputPath);

            return File.Exists(OutputPath);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    protected void VerifyFileIntegrity()
    {
        Assert.True(File.Exists(OutputPath), "Output file should have been created.");

        var outputFileInfo = new FileInfo(OutputPath);
        Assert.True(outputFileInfo.Length > 0, "Output file should not be empty.");
    }
}