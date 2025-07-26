using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class FilterDataAction : ActionBase
{
    [ReplacePlaceholders]
    public string? SheetName { get; set; }
    public required string Range { get; set; }
    public int FilterColumnIndex { get; set; }
    public required string FilterValue { get; set; }
    public string FilterOperator { get; set; } = "Equal"; // Equal, NotEqual, Contains, StartsWith, EndsWith, GreaterThan, LessThan
    public bool CaseSensitive { get; set; } = false; // This property won't be used directly for standard ClosedXML string filters

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            ApplyFilter(worksheet);

            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void ApplyFilter(IXLWorksheet worksheet)
    {
        ValidateInputs();
        var range = GetFilterRange(worksheet);
        var autoFilter = SetupAutoFilter(range);
        ApplyFilterCriteria(autoFilter);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(Range))
            throw new ArgumentException("Range cannot be null or empty.", nameof(Range));

        if (FilterColumnIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(FilterColumnIndex), "Filter column index must be greater than 0.");

        if (string.IsNullOrEmpty(FilterValue))
            throw new ArgumentException("Filter value cannot be null or empty.", nameof(FilterValue));

        if (string.IsNullOrWhiteSpace(FilterOperator))
            throw new ArgumentException("Filter operator cannot be null or empty.", nameof(FilterOperator));
    }

    private IXLRange GetFilterRange(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Range(Range);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid range '{Range}' specified.", ex);
        }
    }

    private static IXLAutoFilter SetupAutoFilter(IXLRange range)
    {
        try
        {
            return range.SetAutoFilter();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to set auto filter on the specified range.", ex);
        }
    }

    private void ApplyFilterCriteria(IXLAutoFilter autoFilter)
    {
        ValidateFilterColumn(autoFilter);
        var filterColumn = autoFilter.Column(FilterColumnIndex);
        ExecuteFilterOperation(filterColumn);
    }

    private void ValidateFilterColumn(IXLAutoFilter autoFilter)
    {
        // Check if the filter column index is within the range of available columns
        var maxColumns = autoFilter.Range.ColumnCount();
        if (FilterColumnIndex > maxColumns)
            throw new ArgumentOutOfRangeException(nameof(FilterColumnIndex),
                $"Filter column index {FilterColumnIndex} exceeds the number of columns in range ({maxColumns}).");
    }

    private void ExecuteFilterOperation(IXLFilterColumn filterColumn)
    {
        var operation = FilterOperator.ToLower().Trim();

        switch (operation)
        {
            case "equal":
                filterColumn.EqualTo(FilterValue);
                break;
            case "notequal":
                filterColumn.NotEqualTo(FilterValue);
                break;
            case "contains":
                filterColumn.Contains(FilterValue);
                break;
            case "startswith":
                filterColumn.BeginsWith(FilterValue);
                break;
            case "endswith":
                filterColumn.EndsWith(FilterValue);
                break;
            case "greaterthan":
                ApplyNumericFilter(filterColumn, FilterValue, (col, val) => col.GreaterThan(val), "GreaterThan");
                break;
            case "lessthan":
                ApplyNumericFilter(filterColumn, FilterValue, (col, val) => col.LessThan(val), "LessThan");
                break;
            default:
                throw new ArgumentException($"Unrecognized filter operator '{FilterOperator}'. " +
                    "Supported operators: Equal, NotEqual, Contains, StartsWith, EndsWith, GreaterThan, LessThan.",
                    nameof(FilterOperator));
        }
    }

    private void ApplyNumericFilter(IXLFilterColumn filterColumn, string value, Action<IXLFilterColumn, double> filterAction, string operatorName)
    {
        if (!double.TryParse(value, out double numericValue))
            throw new ArgumentException($"FilterValue '{value}' is not a valid number for {operatorName} comparison.", nameof(FilterValue));

        filterAction(filterColumn, numericValue);
    }
}