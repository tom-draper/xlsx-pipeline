using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.File;

public class SplitSheetsToFilesAction : ActionBase
{
    [JsonPropertyName("outputDirectory")]
    public required PlaceholderString OutputDirectory { get; set; }

    [JsonPropertyName("fileNamePrefix")]
    public PlaceholderString? FileNamePrefix { get; set; }

    [JsonPropertyName("createDirectories")]
    public bool CreateDirectories { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        string outputDir = OutputDirectory.Resolved;

        if (CreateDirectories && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string prefix = string.IsNullOrEmpty(FileNamePrefix) ? string.Empty : (string)FileNamePrefix!;

        using var workbook = new XLWorkbook(filePath);

        foreach (var sheet in workbook.Worksheets)
        {
            if (sheet.Visibility != XLWorksheetVisibility.Visible)
                continue;

            using var newWorkbook = new XLWorkbook();
            sheet.CopyTo(newWorkbook, sheet.Name);

            string outputPath = Path.Combine(outputDir, $"{prefix}{sheet.Name}.xlsx");
            newWorkbook.SaveAs(outputPath);
        }

        return Task.CompletedTask;
    }
}
