using ClosedXML.Excel;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Cells.ValidateData;

public abstract class ValidateDataTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ValidateDataAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Cells", "ValidateData"), defaultPipelineName)
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

    protected void VerifyDataValidationApplied(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // Verify that data validation was applied to the range
        var validations = worksheet.DataValidations.ToList();
        Assert.NotEmpty(validations);
    }
}
