using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.CopyFile;

public abstract class CopyFileTestBase : PipelineTestBase
{
    protected readonly string OutputPath;

    protected CopyFileTestBase() : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "CopyFile"))
    {
        OutputPath = Path.GetFullPath(GetOutputPath());
    }

    protected override void UpdatePipelinePaths()
    {
        UpdatePipelinePathsForAction<CopyFileAction>();
    }

    private string GetOutputPath()
    {
        return Pipeline.Actions
            .OfType<CopyFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.DestinationPath))?
            .DestinationPath ?? throw new InvalidOperationException("No CopyFileAction with destination found");
    }

    protected async Task<bool> ExecuteCopyFileTestAsync()
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

        ExcelTestHelpers.VerifyFilesAreIdentical(InputPath, OutputPath);
    }
}