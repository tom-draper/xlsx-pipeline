using ClosedXML.Excel;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Cells.CopyRange;

public abstract class CopyRangeTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<CopyRangeAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Cells", "CopyRange"), defaultPipelineName)
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

    protected void VerifyCopyRange(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // Source A1:A3 should have been copied to C1:C3
        Assert.Equal(worksheet.Cell("A1").GetString(), worksheet.Cell("C1").GetString());
        Assert.Equal(worksheet.Cell("A2").GetString(), worksheet.Cell("C2").GetString());
        Assert.Equal(worksheet.Cell("A3").GetString(), worksheet.Cell("C3").GetString());
    }
}
