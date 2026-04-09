using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.InsertRow;

public abstract class InsertRowTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<InsertRowAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "InsertRow"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithRows(inputPath);
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

    private static void CreateTestFileWithRows(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "Row1";
        worksheet.Cell("A2").Value = "Row2";
        worksheet.Cell("A3").Value = "Row3";
        workbook.SaveAs(path);
    }

    protected void VerifyRowInserted(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // After inserting 1 row at row 2, original row 2 ("Row2") should now be at row 3
        Assert.Equal("Row1", worksheet.Cell("A1").GetString());
        Assert.Equal("", worksheet.Cell("A2").GetString());
        Assert.Equal("Row2", worksheet.Cell("A3").GetString());
        Assert.Equal("Row3", worksheet.Cell("A4").GetString());
    }
}
