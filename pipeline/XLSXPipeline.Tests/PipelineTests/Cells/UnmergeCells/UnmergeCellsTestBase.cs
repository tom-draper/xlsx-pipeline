using ClosedXML.Excel;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Cells.UnmergeCells;

public abstract class UnmergeCellsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<UnmergeCellsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Cells", "UnmergeCells"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            // Create a workbook with A1:B2 already merged
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell("A1").Value = "Merged";
                worksheet.Range("A1:B2").Merge();
                workbook.SaveAs(inputPath);
            }

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

    protected void VerifyUnmergeCells(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        var hasA1B2Merged = worksheet.MergedRanges.Any(r =>
            r.FirstCell().Address.RowNumber == 1 &&
            r.FirstCell().Address.ColumnNumber == 1 &&
            r.LastCell().Address.RowNumber == 2 &&
            r.LastCell().Address.ColumnNumber == 2);

        Assert.False(hasA1B2Merged, "Range A1:B2 should no longer be merged.");
    }
}
