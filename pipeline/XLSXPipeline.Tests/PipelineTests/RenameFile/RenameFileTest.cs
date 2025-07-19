using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Extensions;
using XLSXPipeline.Models;
using XLSXPipeline.Services;

namespace XLSXPipeline.Tests.ActionTests.RenameFile;

public class RenameFileTest : IDisposable
{
    private readonly IServiceCollection _services;
    private readonly string _baseDir = @"..\..\..\PipelineTests\RenameFile";
    private readonly Pipeline _pipeline;
    private readonly string _inputPath;
    private readonly string _newName;
    private readonly List<string> _tempFilesToCleanup;

    public RenameFileTest()
    {
        _services = new ServiceCollection();

        // Add logging services
        _services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add pipeline services
        _services.AddPipelineServices();

        var pipelinePath = Path.GetFullPath(Path.Combine(_baseDir, @".\Pipelines\Pipeline.json"));
        _pipeline = CreatePipelineAsync(pipelinePath).GetAwaiter().GetResult();

        _inputPath = Path.GetFullPath(Path.Combine(_baseDir, _pipeline.Trigger.Path));
        _newName = _pipeline.Actions
            .OfType<RenameFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.NewName))?
            .NewName;

        _tempFilesToCleanup = [];
    }

    [Fact]
    public async Task CopyFile_CopiesFileToDestination()
    {
        try
        {
            CreateTestFile(_inputPath);
            _tempFilesToCleanup.Add(_inputPath);
            var outputPath = GetOutputPath();
            _tempFilesToCleanup.Add(outputPath);

            // Arrange
            var serviceProvider = _services.BuildServiceProvider();
            var pipelineExecutor = serviceProvider.GetRequiredService<IPipelineExecutor>();

            // Act
            await pipelineExecutor.ExecutePipelineAsync(_pipeline, _inputPath);

            // Assert
            Assert.True(File.Exists(outputPath), "Output file should have been created.");

            // Verify file properties
            var outputFileInfo = new FileInfo(outputPath);
            Assert.True(outputFileInfo.Length > 0, "Output file should not be empty.");
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    private string GetOutputPath()
    {
        string filename = _newName;
        if (string.IsNullOrEmpty(Path.GetExtension(_newName)))
        {
            var originalExtension = Path.GetExtension(_inputPath);
            filename = Path.ChangeExtension(_newName, originalExtension);
        }

        var outputPath = Path.Combine(Path.GetDirectoryName(_inputPath), filename);
        return outputPath;
    }

    private static async Task<Pipeline> CreatePipelineAsync(string pipelinePath)
    {
        var json = await File.ReadAllTextAsync(pipelinePath);
        return JsonSerializer.Deserialize<Pipeline>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static void CreateTestFile(string path)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");

        // Add diverse test data
        worksheet.Cell("A1").Value = "Test Data";
        worksheet.Cell("B1").Value = "More Data";
        worksheet.Cell("A2").Value = 123;
        worksheet.Cell("B2").Value = 456.789;
        worksheet.Cell("A3").Value = DateTime.Now;
        worksheet.Cell("B3").Value = true;

        workbook.SaveAs(path);
    }

    private async Task CleanupTempFilesAsync()
    {
        await Task.Run(() =>
        {
            foreach (var filePath in _tempFilesToCleanup)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception but don't fail the test
                    System.Diagnostics.Debug.WriteLine($"Failed to cleanup temp file {filePath}: {ex.Message}");
                }
            }
            _tempFilesToCleanup.Clear();
        });
    }

    public void Dispose()
    {
        // Synchronous cleanup as fallback
        foreach (var filePath in _tempFilesToCleanup)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore cleanup errors in dispose
            }
        }
        _tempFilesToCleanup.Clear();
    }
}