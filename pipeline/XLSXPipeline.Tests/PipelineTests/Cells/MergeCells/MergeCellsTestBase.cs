using ClosedXML.Excel;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Cells.MergeCells;

public abstract class MergeCellsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<MergeCellsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Cells", "MergeCells"), defaultPipelineName)
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

    protected void VerifyMergeCells(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        var mergedRanges = worksheet.MergedRanges.ToList();
        Assert.NotEmpty(mergedRanges);

        var hasMergedA1B2 = mergedRanges.Any(r =>
            r.FirstCell().Address.RowNumber == 1 &&
            r.FirstCell().Address.ColumnNumber == 1 &&
            r.LastCell().Address.RowNumber == 2 &&
            r.LastCell().Address.ColumnNumber == 2);

        Assert.True(hasMergedA1B2, "Expected merged range A1:B2 not found.");
    }
}
