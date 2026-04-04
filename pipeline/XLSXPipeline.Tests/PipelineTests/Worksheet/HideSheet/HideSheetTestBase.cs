using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.HideSheet;

public abstract class HideSheetTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<HideSheetAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "HideSheet"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            // Create workbook with 2 sheets so we can hide one
            using (var workbook = new XLWorkbook())
            {
                var sheet1 = workbook.Worksheets.Add("Sheet1");
                sheet1.Cell("A1").Value = "Sheet1 Data";
                var sheet2 = workbook.Worksheets.Add("Sheet2");
                sheet2.Cell("A1").Value = "Sheet2 Data";
                workbook.SaveAs(inputPath);
            }

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

    protected void VerifyHideSheet(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        Assert.Equal(XLWorksheetVisibility.Hidden, workbook.Worksheet("Sheet1").Visibility);
    }
}
