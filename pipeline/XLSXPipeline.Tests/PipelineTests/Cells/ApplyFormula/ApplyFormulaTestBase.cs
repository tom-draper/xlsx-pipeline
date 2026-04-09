using ClosedXML.Excel;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Cells.ApplyFormula;

public abstract class ApplyFormulaTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ApplyFormulaAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Cells", "ApplyFormula"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithNumericData(inputPath);
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

    private static void CreateTestFileWithNumericData(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = 1;
        worksheet.Cell("A2").Value = 2;
        workbook.SaveAs(path);
    }

    protected void VerifyFormulaApplied(string cellAddress, string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();
        var formula = worksheet.Cell(cellAddress).FormulaA1;
        Assert.False(string.IsNullOrEmpty(formula), $"Expected a formula at cell {cellAddress}");
    }
}
