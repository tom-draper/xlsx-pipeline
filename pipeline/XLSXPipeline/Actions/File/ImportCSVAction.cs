using ClosedXML.Excel;
using System.Text;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.File;

public class ImportCSVAction : ActionBase
{
    /// <summary>
    /// Path to the CSV file to import. Supports date/time placeholders.
    /// </summary>
    [JsonPropertyName("csvFilePath")]
    public required PlaceholderString CsvFilePath { get; set; }

    /// <summary>
    /// Name of the destination sheet. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Cell address where import begins. Defaults to "A1".
    /// </summary>
    public string StartCell { get; set; } = "A1";

    /// <summary>
    /// Column delimiter. Defaults to ",".
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Text encoding of the CSV file. Defaults to "utf-8".
    /// </summary>
    public string Encoding { get; set; } = "utf-8";

    /// <summary>
    /// When true, any existing data in the destination area is cleared before import. Defaults to false.
    /// </summary>
    public bool ClearExistingData { get; set; } = false;

    protected override async Task ExecuteInternalAsync(string filePath)
    {
        string csvPath = CsvFilePath.Resolved;
        if (!System.IO.File.Exists(csvPath))
            throw new FileNotFoundException($"CSV file not found: {csvPath}");

        var encoding = GetEncoding();
        var lines = await System.IO.File.ReadAllLinesAsync(csvPath, encoding);

        if (lines.Length == 0)
            return;

        char delimiter = Delimiter.Length == 1 ? Delimiter[0] : ',';

        using var workbook = new XLWorkbook(filePath);
        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        var startCellAddress = worksheet.Cell(StartCell).Address;
        int startRow = startCellAddress.RowNumber;
        int startCol = startCellAddress.ColumnNumber;

        if (ClearExistingData)
        {
            int endRow = startRow + lines.Length - 1;
            int maxCols = lines.Max(l => ParseCsvLine(l, delimiter).Count);
            int endCol = startCol + maxCols - 1;
            if (endRow >= startRow && endCol >= startCol)
                worksheet.Range(startRow, startCol, endRow, endCol).Clear();
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i], delimiter);
            for (int j = 0; j < fields.Count; j++)
            {
                var cell = worksheet.Cell(startRow + i, startCol + j);
                SetCellValue(cell, fields[j]);
            }
        }

        workbook.Save();
    }

    private System.Text.Encoding GetEncoding()
    {
        try { return System.Text.Encoding.GetEncoding(Encoding); }
        catch { throw new ArgumentException($"Unknown encoding '{Encoding}'."); }
    }

    private static List<string> ParseCsvLine(string line, char delimiter)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote inside quoted field
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == delimiter)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    private static void SetCellValue(IXLCell cell, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            cell.Value = string.Empty;
            return;
        }

        // Try numeric types first so imported numbers are stored as numbers, not text
        if (double.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double numVal))
        {
            cell.Value = numVal;
            return;
        }

        if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out DateTime dtVal))
        {
            cell.Value = dtVal;
            return;
        }

        if (bool.TryParse(value, out bool boolVal))
        {
            cell.Value = boolVal;
            return;
        }

        cell.Value = value;
    }
}
