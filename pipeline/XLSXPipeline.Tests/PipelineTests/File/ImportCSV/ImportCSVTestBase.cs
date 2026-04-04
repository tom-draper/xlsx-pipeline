using ClosedXML.Excel;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Tests.Infrastructure;

namespace XLSXPipeline.Tests.PipelineTests.File.ImportCSV;

public abstract class ImportCSVTestBase(string? defaultPipelineName = null) : SpecializedPipelineTestBase<ImportCSVAction>(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "PipelineTests", "File", "ImportCSV"), defaultPipelineName)
{
    protected override void UpdatePipelinePaths(Models.Pipeline pipeline)
    {
        base.UpdatePipelinePaths(pipeline);
        // Also update CsvFilePath property
        foreach (var action in pipeline.Actions.OfType<ImportCSVAction>())
            UpdatePipelinePropertyForAction(action, "CsvFilePath");
    }

    protected override async Task ExecutePipelineTestAsync(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var pipeline = GetPipeline(pipelineName);
        var inputPath = GetInputPath(pipelineName);

        try
        {
            // Create the XLSX input file
            ExcelTestHelpers.CreateTestFile(inputPath);
            AddTempFile(inputPath);

            // Create the CSV file
            var csvPath = Path.Combine(BaseDir, "Input", "Data.csv");
            await System.IO.File.WriteAllTextAsync(csvPath, "Name,Age\nAlice,30\nBob,25");
            AddTempFile(csvPath);

            var pipelineExecutor = GetPipelineExecutor();
            await pipelineExecutor.ExecutePipelineAsync(pipeline, inputPath);
        }
        catch (Exception)
        {
            await CleanupTempFilesAsync();
            throw;
        }
    }

    protected void VerifyImportCSV(string? pipelineName = null)
    {
        pipelineName ??= DefaultPipelineName;
        var inputPath = GetInputPath(pipelineName);

        using var workbook = new XLWorkbook(inputPath);
        var worksheet = workbook.Worksheet("Sheet1");

        Assert.Equal("Name", worksheet.Cell("A1").GetString());
        Assert.Equal("Age", worksheet.Cell("B1").GetString());
        Assert.Equal("Alice", worksheet.Cell("A2").GetString());
        Assert.Equal("30", worksheet.Cell("B2").GetString());
    }
}
