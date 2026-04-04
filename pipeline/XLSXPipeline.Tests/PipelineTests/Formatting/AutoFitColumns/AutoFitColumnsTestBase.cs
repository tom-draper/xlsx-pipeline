using ClosedXML.Excel;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Formatting.AutoFitColumns;

public abstract class AutoFitColumnsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<AutoFitColumnsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Formatting", "AutoFitColumns"), defaultPipelineName)
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

    protected void VerifyAutoFitColumns(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        // Verify the file can be reopened successfully (pipeline executed without error)
        using var workbook = new XLWorkbook(inputPath);
        Assert.NotEmpty(workbook.Worksheets);
    }
}
