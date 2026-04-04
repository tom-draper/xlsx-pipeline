using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.SetHeaderFooter;

public abstract class SetHeaderFooterTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<SetHeaderFooterAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "SetHeaderFooter"), defaultPipelineName)
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

    protected void VerifySetHeaderFooter(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // Check all occurrences for header text (AddText stores per occurrence)
        var allPagesText = worksheet.PageSetup.Header.Center.GetText(XLHFOccurrence.AllPages);
        var firstPageText = worksheet.PageSetup.Header.Center.GetText(XLHFOccurrence.FirstPage);
        var oddPagesText = worksheet.PageSetup.Header.Center.GetText(XLHFOccurrence.OddPages);
        var evenPagesText = worksheet.PageSetup.Header.Center.GetText(XLHFOccurrence.EvenPages);

        var hasText = !string.IsNullOrEmpty(allPagesText)
            || !string.IsNullOrEmpty(firstPageText)
            || !string.IsNullOrEmpty(oddPagesText)
            || !string.IsNullOrEmpty(evenPagesText);

        Assert.True(hasText, "Center header should not be empty for any page occurrence.");
    }
}
