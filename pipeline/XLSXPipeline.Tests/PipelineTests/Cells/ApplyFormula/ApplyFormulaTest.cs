using ClosedXML.Excel;

namespace XLSXPipeline.Tests.PipelineTests.Cells.ApplyFormula;

[Collection("FileAccess")]
public class ApplyFormulaTest : ApplyFormulaTestBase
{
    [Theory]
    [InlineData("Apply Formula")]
    public async Task ApplyFormula_SpecificPipeline_ShouldSucceed(string pipelineName)
    {
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            VerifyFormulaApplied("C1", pipelineName);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    [Fact]
    public async Task ApplyFormula_WithPlaceholders_ShouldResolvePlaceholders()
    {
        string pipelineName = "Apply Formula Placeholders";
        try
        {
            await ExecutePipelineTestAsync(pipelineName);
            
            var now = DateTime.Now;
            // Observed behavior: FormulaA1 might return the formula without the leading '='
            string expectedFormula = $"\"Current Date: \" & \"{now.Year:D4}-{now.Month:D2}\"";
            
            var inputPath = GetInputPath(pipelineName);
            using var workbook = new XLWorkbook(inputPath);
            var worksheet = workbook.Worksheets.First();
            var actualFormula = worksheet.Cell("B1").FormulaA1;
            
            Assert.Equal(expectedFormula, actualFormula);
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }
}
