using ClosedXML.Excel;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Formatting.FormatCells;

public abstract class FormatCellsTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<FormatCellsAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Formatting", "FormatCells"), defaultPipelineName)
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

    protected void VerifyCellsFormatted(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        Assert.True(worksheet.Cell("A1").Style.Font.Bold, "Expected cell A1 to be bold");
        Assert.Equal(14, worksheet.Cell("A1").Style.Font.FontSize);
    }
}
