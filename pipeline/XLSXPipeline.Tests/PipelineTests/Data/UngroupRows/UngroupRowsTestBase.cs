using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.UngroupRows;

public abstract class UngroupRowsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<UngroupRowsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "UngroupRows"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            // Create a workbook with rows 2-4 already grouped
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");
                worksheet.Cell("A1").Value = "Header";
                worksheet.Cell("A2").Value = "Row2";
                worksheet.Cell("A3").Value = "Row3";
                worksheet.Cell("A4").Value = "Row4";
                worksheet.Rows(2, 4).Group();
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

    protected void VerifyUngroupRows(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        Assert.Equal(0, worksheet.Row(2).OutlineLevel);
    }
}
