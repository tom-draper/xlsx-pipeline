using ClosedXML.Excel;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Data.MergeData;

public abstract class MergeDataTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<MergeDataAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Data", "MergeData"), defaultPipelineName)
{
    protected string SourceFilePath => Path.GetFullPath(Path.Combine(BaseDir, "Source", "Source.xlsx"));

    protected override void UpdatePipelinePaths(XLSXPipeline.Models.Pipeline pipeline)
    {
        base.UpdatePipelinePaths(pipeline);
        // Also update SourceFilePath which is not covered by the default property names
        foreach (var action in pipeline.Actions.OfType<MergeDataAction>())
            UpdatePipelinePropertyForAction(action, "SourceFilePath");
    }

    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateDestinationFile(inputPath);
            AddTempFile(inputPath);

            CreateSourceFile(SourceFilePath);
            AddTempFile(SourceFilePath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    private static void CreateDestinationFile(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "Header";
        worksheet.Cell("A2").Value = "DestRow1";
        worksheet.Cell("A3").Value = "DestRow2";
        workbook.SaveAs(path);
    }

    private static void CreateSourceFile(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "SourceHeader";
        worksheet.Cell("A2").Value = "SourceRow1";
        worksheet.Cell("A3").Value = "SourceRow2";
        workbook.SaveAs(path);
    }

    protected void VerifyDataMerged(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // Original data in rows 1-3, merged data (without headers, includeHeaders=false)
        // starts at A4, so SourceRow1 should be at A4 and SourceRow2 at A5
        Assert.Equal("SourceRow1", worksheet.Cell("A4").GetString());
        Assert.Equal("SourceRow2", worksheet.Cell("A5").GetString());
    }
}
