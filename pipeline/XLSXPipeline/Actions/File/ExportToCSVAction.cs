using ClosedXML.Excel;
using System.Text;

namespace XLSXPipeline.Actions.File;

public class ExportToCSVAction : ActionBase
{
    public string? OutputPath { get; set; }
    public string? FileName { get; set; }
    public string? SheetName { get; set; }
    public string Delimiter { get; set; } = ",";
    public string Encoding { get; set; } = "utf-8";
    public bool IncludeHeaders { get; set; } = true;
    public bool TrimWhitespace { get; set; } = true;
    public bool ReplaceFile { get; set; } = false;

    protected override async Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);

            using var workbook = new XLWorkbook(filePath);
            var worksheet = GetWorksheet(workbook);
            var csvContent = BuildCsvContent(worksheet);
            var outputPath = DetermineOutputPath(filePath, OutputPath, FileName);

            await System.IO.File.WriteAllTextAsync(outputPath, csvContent, GetEncodingInstance());

            if (ReplaceFile)
                System.IO.File.Delete(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export to CSV: {ex.Message}", ex);
        }
    }

    private void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Source file path cannot be null or empty");

        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Source file not found: {filePath}");
    }

    public static string DetermineOutputPath(string sourceFilePath, string? outputPath, string? filename)
    {
        string outputDirectory;
        string outputFilename;

        // Determine the output directory
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            if (Path.HasExtension(outputPath))
                // OutputPath includes a filename, extract directory and ignore the filename part
                outputDirectory = Path.GetDirectoryName(outputPath) ?? string.Empty;
            else
                // OutputPath is just a directory
                outputDirectory = outputPath;
        }
        else
        {
            // Default to same directory as source file
            outputDirectory = Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
        }

        // Determine the filename
        if (!string.IsNullOrWhiteSpace(filename))
        {
            // Use the explicitly provided FileName
            outputFilename = Path.HasExtension(filename) ? filename : filename + ".csv";
        }
        else
        {
            // Default to source filename with .csv extension
            var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            outputFilename = sourceFileName + ".csv";
        }

        var fullOutputPath = Path.Combine(outputDirectory, outputFilename);
        var resolvedPath = Path.GetFullPath(fullOutputPath);

        // Ensure the output directory exists
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return resolvedPath;
    }

    private IXLWorksheet GetWorksheet(XLWorkbook workbook)
    {
        if (string.IsNullOrEmpty(SheetName))
            return workbook.Worksheets.First();

        var worksheet = workbook.Worksheet(SheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Sheet '{SheetName}' does not exist.");

        return worksheet;
    }

    private string BuildCsvContent(IXLWorksheet worksheet)
    {
        var csvContent = new StringBuilder();
        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
            return string.Empty;

        int startRow = IncludeHeaders ? 1 : 2;
        int lastRow = usedRange.LastRow().RowNumber();
        int lastColumn = usedRange.LastColumn().ColumnNumber();

        for (int row = startRow; row <= lastRow; row++)
        {
            var rowValues = BuildRowValues(worksheet, row, lastColumn);
            csvContent.AppendLine(string.Join(Delimiter, rowValues));
        }

        return csvContent.ToString();
    }

    private List<string> BuildRowValues(IXLWorksheet worksheet, int row, int lastColumn)
    {
        var rowValues = new List<string>();

        for (int col = 1; col <= lastColumn; col++)
        {
            var cell = worksheet.Cell(row, col);
            var cellValue = GetCellValueAsString(cell);

            if (TrimWhitespace)
                cellValue = cellValue.Trim();

            cellValue = EscapeCsvValue(cellValue);
            rowValues.Add(cellValue);
        }

        return rowValues;
    }

    private Encoding GetEncodingInstance()
    {
        return System.Text.Encoding.GetEncoding(Encoding);
    }

    private static string GetCellValueAsString(IXLCell cell)
    {
        if (cell.IsEmpty())
            return string.Empty;

        return cell.DataType switch
        {
            XLDataType.DateTime => cell.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss"),
            XLDataType.TimeSpan => cell.GetTimeSpan().ToString(),
            XLDataType.Boolean => cell.GetBoolean().ToString().ToLower(),
            XLDataType.Number => cell.GetDouble().ToString(),
            _ => cell.GetString()
        };
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        bool needsEscaping = value.Contains(Delimiter) ||
                           value.Contains('"') ||
                           value.Contains('\n') ||
                           value.Contains('\r');

        if (!needsEscaping)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}