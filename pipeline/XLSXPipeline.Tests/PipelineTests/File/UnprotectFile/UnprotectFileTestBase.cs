using ClosedXML.Excel;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.UnprotectFile;

public abstract class UnprotectFileTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<UnprotectFileAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "UnprotectFile"), defaultPipelineName)
{
    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            CreateProtectedTestFile(inputPath, "testpass123");
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

    private static void CreateProtectedTestFile(string path, string password)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");
        worksheet.Cell("A1").Value = "Test Data";
        workbook.Protect(password);
        workbook.SaveAs(path);
    }

    protected void VerifyFileIsUnprotected(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        Assert.True(System.IO.File.Exists(inputPath), $"Expected file to exist: {inputPath}");

        using var workbook = new XLWorkbook(inputPath);
        Assert.False(workbook.IsProtected, "Expected workbook to be unprotected after action");
    }
}
