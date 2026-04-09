using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.InsertColumn;

public abstract class InsertColumnTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<InsertColumnAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "InsertColumn"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithColumns(inputPath);
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

    private static void CreateTestFileWithColumns(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "ColA";
        worksheet.Cell("B1").Value = "ColB";
        worksheet.Cell("C1").Value = "ColC";
        workbook.SaveAs(path);
    }

    protected void VerifyColumnInserted(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // After inserting 1 column after B, original B ("ColB") stays in B,
        // new empty column appears at C, original C ("ColC") shifts to D
        Assert.Equal("ColA", worksheet.Cell("A1").GetString());
        Assert.Equal("ColB", worksheet.Cell("B1").GetString());
        Assert.Equal("", worksheet.Cell("C1").GetString());
        Assert.Equal("ColC", worksheet.Cell("D1").GetString());
    }
}
