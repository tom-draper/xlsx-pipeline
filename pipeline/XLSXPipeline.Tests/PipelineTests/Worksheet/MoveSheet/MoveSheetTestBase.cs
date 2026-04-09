using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.MoveSheet;

public abstract class MoveSheetTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<MoveSheetAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "MoveSheet"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithTwoSheets(inputPath);
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

    private static void CreateTestFileWithTwoSheets(string path)
    {
        using var workbook = new XLWorkbook();
        var sheet1 = workbook.Worksheets.Add("Sheet1");
        sheet1.Cell("A1").Value = "Sheet1 Data";
        var sheet2 = workbook.Worksheets.Add("Sheet2");
        sheet2.Cell("A1").Value = "Sheet2 Data";
        workbook.SaveAs(path);
    }

    protected void VerifySheetMoved(string movedSheetName, int expectedPosition, string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheet(movedSheetName);
        Assert.NotNull(worksheet);
        Assert.Equal(expectedPosition, worksheet.Position);
    }
}
