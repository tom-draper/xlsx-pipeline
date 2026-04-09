using ClosedXML.Excel;
using XLSXPipeline.Actions.Advanced;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Advanced.Transpose;

public abstract class TransposeTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<TransposeAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Advanced", "Transpose"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateTestFileWithData(inputPath);
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

    private static void CreateTestFileWithData(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        // 2x2 grid: A1=1, B1=2, A2=3, B2=4
        worksheet.Cell("A1").Value = 1;
        worksheet.Cell("B1").Value = 2;
        worksheet.Cell("A2").Value = 3;
        worksheet.Cell("B2").Value = 4;
        workbook.SaveAs(path);
    }

    protected void VerifyTransposed(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheets.First();

        // Source A1:B2 transposed to D1:
        // Source[0,0]=1 -> dest D1, Source[0,1]=2 -> dest D2
        // Source[1,0]=3 -> dest E1, Source[1,1]=4 -> dest E2
        Assert.Equal("1", worksheet.Cell("D1").GetString());
        Assert.Equal("2", worksheet.Cell("D2").GetString());
        Assert.Equal("3", worksheet.Cell("E1").GetString());
        Assert.Equal("4", worksheet.Cell("E2").GetString());
    }
}
