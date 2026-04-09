using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.AddSheet;

public abstract class AddSheetTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<AddSheetAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "AddSheet"), defaultPipelineName)
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

    protected void VerifySheetAdded(string expectedSheetName, string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        Assert.True(workbook.Worksheets.Contains(expectedSheetName), $"Expected sheet '{expectedSheetName}' to exist in workbook");
    }
}
