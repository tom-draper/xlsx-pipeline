using ClosedXML.Excel;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.SplitSheetsToFiles;

public abstract class SplitSheetsToFilesTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<SplitSheetsToFilesAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "SplitSheetsToFiles"), defaultPipelineName)
{
    protected override void UpdatePipelinePaths(Models.Pipeline pipeline)
    {
        base.UpdatePipelinePaths(pipeline);
        foreach (var action in pipeline.Actions.OfType<SplitSheetsToFilesAction>())
            UpdatePipelinePropertyForAction(action, "OutputDirectory");
    }

    protected string GetOutputDirectory(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var action = pipeline.Actions.OfType<SplitSheetsToFilesAction>().First();
        return action.OutputDirectory.Resolved;
    }

    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);
        var outputDir = GetOutputDirectory(pipelineName);

        try
        {
            // Create workbook with 2 sheets
            using (var workbook = new XLWorkbook())
            {
                var sheet1 = workbook.Worksheets.Add("Sheet1");
                sheet1.Cell("A1").Value = "Sheet1 Data";
                var sheet2 = workbook.Worksheets.Add("Sheet2");
                sheet2.Cell("A1").Value = "Sheet2 Data";
                workbook.SaveAs(inputPath);
            }

            AddTempFile(inputPath);

            // Register expected output files for cleanup
            AddTempFile(Path.Combine(outputDir, "Sheet1.xlsx"));
            AddTempFile(Path.Combine(outputDir, "Sheet2.xlsx"));

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifySplitSheetsToFiles(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var outputDir = GetOutputDirectory(pipelineName);

        var sheet1Path = Path.Combine(outputDir, "Sheet1.xlsx");
        var sheet2Path = Path.Combine(outputDir, "Sheet2.xlsx");

        Assert.True(System.IO.File.Exists(sheet1Path), $"Expected output file '{sheet1Path}' does not exist.");
        Assert.True(System.IO.File.Exists(sheet2Path), $"Expected output file '{sheet2Path}' does not exist.");
    }
}
