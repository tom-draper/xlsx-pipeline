using ClosedXML.Excel;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Cells.ClearCells;

public abstract class ClearCellsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ClearCellsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Cells", "ClearCells"), defaultPipelineName)
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

    protected void VerifyClearCells(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        Assert.Equal(string.Empty, worksheet.Cell("A1").GetString());
        Assert.Equal(string.Empty, worksheet.Cell("A2").GetString());
        Assert.Equal(string.Empty, worksheet.Cell("B1").GetString());
        Assert.Equal(string.Empty, worksheet.Cell("B2").GetString());
    }
}
