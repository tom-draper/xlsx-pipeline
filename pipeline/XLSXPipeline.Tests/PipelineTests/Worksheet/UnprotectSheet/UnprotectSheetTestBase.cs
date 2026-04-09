using ClosedXML.Excel;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.Worksheet.UnprotectSheet;

public abstract class UnprotectSheetTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<UnprotectSheetAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "Worksheet", "UnprotectSheet"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateProtectedSheetFile(inputPath, "sheetpass");
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

    private static void CreateProtectedSheetFile(string path, string password)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "Protected Data";
        worksheet.Protect(password);
        workbook.SaveAs(path);
    }

    protected void VerifySheetUnprotected(string sheetName, string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheet(sheetName);
        Assert.NotNull(worksheet);
        Assert.False(worksheet.Protection.IsProtected, $"Expected sheet '{sheetName}' to be unprotected");
    }
}
