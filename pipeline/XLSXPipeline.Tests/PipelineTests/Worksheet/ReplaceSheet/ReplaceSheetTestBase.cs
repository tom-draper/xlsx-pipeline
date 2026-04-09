using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.ReplaceSheet;

public abstract class ReplaceSheetTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ReplaceSheetAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "ReplaceSheet"), defaultPipelineName)
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
        sheet1.Cell("A1").Value = "Original Sheet1 Data";
        var source = workbook.Worksheets.Add("Source");
        source.Cell("A1").Value = "Source Replacement Data";
        workbook.SaveAs(path);
    }

    protected void VerifySheetReplaced(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        // Sheet1 still exists but now has the content from Source
        Assert.True(workbook.Worksheets.Contains("Sheet1"), "Expected 'Sheet1' to still exist after replace");
        var sheet1 = workbook.Worksheet("Sheet1");
        Assert.Equal("Source Replacement Data", sheet1.Cell("A1").GetString());
    }
}
