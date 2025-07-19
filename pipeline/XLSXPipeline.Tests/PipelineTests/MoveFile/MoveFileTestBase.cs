using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.MoveFile;

public abstract class MoveFileTestBase : PipelineTestBase
{
    protected readonly string OutputPath;

    protected MoveFileTestBase() : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "MoveFile"))
    {
        OutputPath = Path.GetFullPath(GetOutputPath());
    }

    protected override void UpdatePipelinePaths()
    {
        UpdatePipelinePathsForAction<MoveFileAction>();
    }

    private string GetOutputPath()
    {
        return Pipeline.Actions
            .OfType<MoveFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.DestinationPath))?
            .DestinationPath ?? throw new InvalidOperationException("No MoveFileAction with destination found");
    }

    protected async Task<bool> ExecuteMoveFileTestAsync()
    {
        try
        {
            ExcelTestHelpers.CreateTestFile(InputPath);
            AddTempFile(InputPath);
            AddTempFile(OutputPath);

            var pipelineExecutor = await GetPipelineExecutorAsync();
            await pipelineExecutor.ExecutePipelineAsync(Pipeline, InputPath);

            return File.Exists(OutputPath) && !File.Exists(InputPath);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    protected void VerifyMovedFile()
    {
        Assert.False(File.Exists(InputPath), "Input file should no longer exist after move.");
        Assert.True(File.Exists(OutputPath), "Output file should exist after move.");

        var outputFileInfo = new FileInfo(OutputPath);
        Assert.True(outputFileInfo.Length > 0, "Moved file should not be empty.");
    }
}