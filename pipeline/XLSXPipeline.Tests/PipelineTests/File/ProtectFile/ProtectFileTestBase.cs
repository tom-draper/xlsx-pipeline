using ClosedXML.Excel;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.ProtectFile;

public abstract class ProtectFileTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ProtectFileAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "ProtectFile"), defaultPipelineName)
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

    protected void VerifyFileIsProtected(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        Assert.True(System.IO.File.Exists(inputPath), $"Expected protected file to exist: {inputPath}");

        using var workbook = new XLWorkbook(inputPath);
        Assert.True(workbook.IsProtected, "Expected workbook to be protected");
    }
}
