using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Extensions;
using XLSXPipeline.Models;
using XLSXPipeline.Services;

namespace XLSXPipeline.Tests.ActionTests.MoveFile;

public class MoveFileTest : IDisposable
{
    private readonly IServiceCollection _services;
    private readonly string _baseDir = @"..\..\..\PipelineTests\MoveFile";
    private readonly Pipeline _pipeline;
    private readonly string _inputPath;
    private readonly string _outputPath;
    private readonly List<string> _tempFilesToCleanup;

    public MoveFileTest()
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

        UpdateDestinationPath(); // Update pipeline destination to base path

        _inputPath = Path.GetFullPath(Path.Combine(_baseDir, _pipeline.Trigger.Path));
        _outputPath = Path.GetFullPath(_pipeline.Actions
            .OfType<MoveFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.DestinationPath))?
            .DestinationPath);

        _tempFilesToCleanup = [];
    }

    [Fact]
    public async Task CopyFile_CopiesFileToDestination()
    {
        try
        {
            CreateTestFile(_inputPath);
            _tempFilesToCleanup.Add(_inputPath);
            _tempFilesToCleanup.Add(_outputPath);

            // Arrange
            var serviceProvider = _services.BuildServiceProvider();
            var pipelineExecutor = serviceProvider.GetRequiredService<IPipelineExecutor>();

            // Act
            await pipelineExecutor.ExecutePipelineAsync(_pipeline, _inputPath);

            // Assert
            Assert.False(File.Exists(_inputPath), "Input file should no longer exist.");
            Assert.True(File.Exists(_outputPath), "Output file should have been created.");

            // Verify file properties
            var inputFileInfo = new FileInfo(_inputPath);
            var outputFileInfo = new FileInfo(_outputPath);
            Assert.True(outputFileInfo.Length > 0, "Output file should not be empty.");
        }
        finally
        {
            await CleanupTempFilesAsync();
        }
    }

    private static async Task<Pipeline> CreatePipelineAsync(string pipelinePath)
    {
        var json = await File.ReadAllTextAsync(pipelinePath);
        return JsonSerializer.Deserialize<Pipeline>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private void UpdateDestinationPath()
    {
        var copyAction = _pipeline.Actions
            .OfType<MoveFileAction>()
            .FirstOrDefault(x => !string.IsNullOrEmpty(x.DestinationPath));

        if (copyAction != null)
        {
            // Combine and normalize the new path
            string fullPath = Path.Combine(_baseDir, copyAction.DestinationPath);

            // Set the updated path
            copyAction.DestinationPath = fullPath;
        }
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