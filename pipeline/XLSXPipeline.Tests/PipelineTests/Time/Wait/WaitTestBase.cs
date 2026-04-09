using XLSXPipeline.Actions.Time;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Time.Wait;

public abstract class WaitTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<WaitAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Time", "Wait"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyWaitCompleted(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        // Wait action doesn't modify the file, just verify it still exists
        Assert.True(System.IO.File.Exists(inputPath), "Expected input file to still exist after wait");
    }
}
