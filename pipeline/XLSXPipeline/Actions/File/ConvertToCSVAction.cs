using ClosedXML.Excel;
using System.Text;

namespace XLSXPipeline.Actions.File;

public class ConvertToCSVAction : ActionBase
{
    public required string OutputPath { get; set; }
    public string? SheetName { get; set; }
    public string Delimiter { get; set; } = ",";
    public string Encoding { get; set; } = "utf-8";
    public bool IncludeHeaders { get; set; } = true;
    public bool TrimWhitespace { get; set; } = true;

    protected override async Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OutputPath))
                throw new ArgumentException("Output path cannot be null or empty", nameof(OutputPath));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Source file path cannot be null or empty");

            // Validate source file exists
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"Source file not found: {filePath}");

            using var workbook = new XLWorkbook(filePath);
            var worksheet = string.IsNullOrEmpty(SheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(SheetName);

            var csvContent = new StringBuilder();
            var usedRange = worksheet.RangeUsed();

            if (usedRange != null)
            {
                int startRow = IncludeHeaders ? 1 : 2;

                for (int row = startRow; row <= usedRange.LastRow().RowNumber(); row++)
                {
                    var rowValues = new List<string>();

                    for (int col = 1; col <= usedRange.LastColumn().ColumnNumber(); col++)
                    {
                        var cell = worksheet.Cell(row, col);
                        var cellValue = GetCellValueAsString(cell);

                        if (TrimWhitespace)
                            cellValue = cellValue.Trim();

                        // Escape CSV value if needed
                        cellValue = EscapeCsvValue(cellValue, Delimiter);
                        rowValues.Add(cellValue);
                    }

                    csvContent.AppendLine(string.Join(Delimiter, rowValues));
                }
            }

            var outputPath = GetOutputPath();
            await System.IO.File.WriteAllTextAsync(outputPath, csvContent.ToString(), System.Text.Encoding.GetEncoding(Encoding));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert to CSV: {ex.Message}", ex);
        }
    }

    private string GetOutputPath()
    {
        var rawPath = Path.HasExtension(OutputPath) ? OutputPath : OutputPath + ".csv";
        var outputPath = Path.GetFullPath(rawPath);  // Normalize and resolve relative path

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return outputPath;
    }

    private static string GetCellValueAsString(IXLCell cell)
    {
        if (cell.IsEmpty())
            return string.Empty;

        // Handle different data types appropriately
        return cell.DataType switch
        {
            XLDataType.DateTime => cell.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss"),
            XLDataType.TimeSpan => cell.GetTimeSpan().ToString(),
            XLDataType.Boolean => cell.GetBoolean().ToString().ToLower(),
            XLDataType.Number => cell.GetDouble().ToString(),
            _ => cell.GetString()
        };
    }

    private static string EscapeCsvValue(string value, string delimiter)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Check if escaping is needed
        bool needsEscaping = value.Contains(delimiter) ||
                           value.Contains('"') ||
                           value.Contains('\n') ||
                           value.Contains('\r');

        if (!needsEscaping)
            return value;

        // Escape quotes by doubling them and wrap in quotes
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}