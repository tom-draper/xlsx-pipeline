using ClosedXML.Excel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace XLSXPipeline.Actions.Cells;

public class FindAndReplaceAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to search. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Text to search for.
    /// </summary>
    public required string Find { get; set; }

    /// <summary>
    /// Replacement text. Use an empty string to delete matched text.
    /// </summary>
    public required string Replace { get; set; }

    /// <summary>
    /// When true, the search is case-sensitive. Defaults to false.
    /// </summary>
    public bool MatchCase { get; set; } = false;

    /// <summary>
    /// When true, the entire cell value must match Find. Defaults to false (substring match).
    /// </summary>
    public bool MatchEntireCell { get; set; } = false;

    /// <summary>
    /// Optional range to limit the search, e.g. "A1:D100". Searches the whole sheet if not set.
    /// </summary>
    public string? SearchRange { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        if (string.IsNullOrEmpty(Find))
            throw new ArgumentException("Find cannot be empty.", nameof(Find));

        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        var cells = GetSearchCells(worksheet);
        var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach (var cell in cells)
        {
            if (cell.IsEmpty() || cell.HasFormula)
                continue;

            var value = cell.GetString();
            if (!IsMatch(value, comparison))
                continue;

            cell.Value = GetReplacement(value, comparison);
        }

        workbook.Save();
        return Task.CompletedTask;
    }

    private IEnumerable<IXLCell> GetSearchCells(IXLWorksheet worksheet)
    {
        if (!string.IsNullOrWhiteSpace(SearchRange))
            return worksheet.Range(SearchRange).Cells();

        return worksheet.CellsUsed();
    }

    private bool IsMatch(string cellValue, StringComparison comparison)
    {
        return MatchEntireCell
            ? string.Equals(cellValue, Find, comparison)
            : cellValue.Contains(Find, comparison);
    }

    private string GetReplacement(string cellValue, StringComparison comparison)
    {
        if (MatchEntireCell)
            return Replace;

        if (MatchCase)
            return cellValue.Replace(Find, Replace, comparison);

        // Case-insensitive substring replacement via Regex
        return Regex.Replace(
            cellValue,
            Regex.Escape(Find),
            Replace.Replace("$", "$$"),   // escape $ in replacement to treat literally
            RegexOptions.IgnoreCase);
    }
}
