using ClosedXML.Excel;
using XLSXPipeline.Actions.Advanced;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Advanced.CreatePivotTable;

public abstract class CreatePivotTableTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<CreatePivotTableAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Advanced", "CreatePivotTable"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithPivotData(inputPath);
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

    private static void CreateTestFileWithPivotData(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        // Header row
        worksheet.Cell("A1").Value = "Category";
        worksheet.Cell("B1").Value = "Value";
        // Data rows
        worksheet.Cell("A2").Value = "Alpha";
        worksheet.Cell("B2").Value = 10;
        worksheet.Cell("A3").Value = "Beta";
        worksheet.Cell("B3").Value = 20;
        worksheet.Cell("A4").Value = "Alpha";
        worksheet.Cell("B4").Value = 30;
        workbook.SaveAs(path);
    }

    protected void VerifyPivotTableCreated(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var pivotSheet = workbook.Worksheet("Pivot");
        Assert.NotNull(pivotSheet);
        Assert.NotEmpty(pivotSheet.PivotTables);
    }
}
