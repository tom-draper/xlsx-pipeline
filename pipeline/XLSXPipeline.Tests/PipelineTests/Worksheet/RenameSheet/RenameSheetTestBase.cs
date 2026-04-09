using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.RenameSheet;

public abstract class RenameSheetTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<RenameSheetAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "RenameSheet"), defaultPipelineName)
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

    protected void VerifySheetRenamed(string oldName, string newName, string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        Assert.True(workbook.Worksheets.Contains(newName), $"Expected renamed sheet '{newName}' to exist");
        Assert.False(workbook.Worksheets.Contains(oldName), $"Expected old sheet name '{oldName}' to no longer exist");
    }
}
