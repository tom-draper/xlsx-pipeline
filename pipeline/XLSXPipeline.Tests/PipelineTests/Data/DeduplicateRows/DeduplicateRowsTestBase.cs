using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.DeduplicateRows;

public abstract class DeduplicateRowsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<DeduplicateRowsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "DeduplicateRows"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            // Create a file with a header row and duplicate data rows
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell("A1").Value = "Name";
                worksheet.Cell("B1").Value = "Value";
                // Row 2 and 3 are duplicates
                worksheet.Cell("A2").Value = "Alice";
                worksheet.Cell("B2").Value = "100";
                worksheet.Cell("A3").Value = "Alice";
                worksheet.Cell("B3").Value = "100";
                // Row 4 is different
                worksheet.Cell("A4").Value = "Bob";
                worksheet.Cell("B4").Value = "200";
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

    protected void VerifyDeduplicateRows(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // Should have header + 2 unique data rows = 3 rows total (not 4)
        var lastRow = worksheet.RangeUsed()?.LastRow().RowNumber() ?? 0;
        Assert.Equal(3, lastRow);
    }
}
