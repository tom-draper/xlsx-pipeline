using XLSXPipeline.Actions.File;
using XLSXPipeline.Models;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.RenameFile;

public abstract class RenameFileTestBase : PipelineTestBase
{
    protected readonly string NewName;
    protected readonly string OutputPath;

    protected RenameFileTestBase() : base(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "RenameFile"))
    {
        NewName = GetRenameFileNewName();
        OutputPath = GetOutputPath();
    }

    private string GetRenameFileNewName()
    {
        return Pipeline.Actions
            .OfType<RenameFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.NewName))?
            .NewName ?? throw new InvalidOperationException("No RenameFileAction with NewName found");
    }

    private string GetOutputPath()
    {
        string filename = NewName;
        if (string.IsNullOrEmpty(Path.GetExtension(NewName)))
        {
            var originalExtension = Path.GetExtension(InputPath);
            filename = Path.ChangeExtension(NewName, originalExtension);
        }

        return Path.Combine(Path.GetDirectoryName(InputPath), filename);
    }

    protected async Task<bool> ExecuteRenameFileTestAsync()
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

    protected void VerifyRenamedFile()
    {
        Assert.True(File.Exists(OutputPath), "Renamed file should exist.");

        var outputFileInfo = new FileInfo(OutputPath);
        Assert.True(outputFileInfo.Length > 0, "Renamed file should not be empty.");

        // Verify the original file no longer exists (unless it's the same path)
        if (!string.Equals(InputPath, OutputPath, StringComparison.OrdinalIgnoreCase))
        {
            Assert.False(File.Exists(InputPath), "Original file should no longer exist after rename.");
        }
    }
}