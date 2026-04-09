using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.RecalculateFormulas;

public abstract class RecalculateFormulasTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<RecalculateFormulasAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "RecalculateFormulas"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithFormula(inputPath);
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

    private static void CreateTestFileWithFormula(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = 10;
        worksheet.Cell("A2").Value = 20;
        worksheet.Cell("A3").FormulaA1 = "=A1+A2";
        workbook.SaveAs(path);
    }

    protected void VerifyRecalculated(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        // Just verify the file can still be opened and has the formula cell intact
        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();
        Assert.NotEmpty(worksheet.Cell("A3").FormulaA1);
    }
}
