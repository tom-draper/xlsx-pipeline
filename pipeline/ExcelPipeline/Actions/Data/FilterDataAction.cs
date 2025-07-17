using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Data
{
    public class FilterDataAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string Range { get; set; }
        public int FilterColumnIndex { get; set; }
        public string FilterValue { get; set; }
        public string FilterOperator { get; set; } = "Equal"; // Equal, NotEqual, Contains, StartsWith, EndsWith, GreaterThan, LessThan
        public bool CaseSensitive { get; set; } = false; // This property won't be used directly for standard ClosedXML string filters

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                var range = worksheet.Range(Range);
                // Set AutoFilter on the specified range
                var autoFilter = range.SetAutoFilter();

                // Get the column to filter by its index
                // Note: FilterColumnIndex is 1-based in ClosedXML
                var filterColumn = autoFilter.Column(FilterColumnIndex);

                switch (FilterOperator.ToLower())
                {
                    case "equal":
                        // ClosedXML's EqualTo method for string values is typically case-insensitive by default.
                        filterColumn.EqualTo(FilterValue);
                        break;
                    case "notequal":
                        // ClosedXML's NotEqualTo method for string values is typically case-insensitive by default.
                        filterColumn.NotEqualTo(FilterValue);
                        break;
                    case "contains":
                        // ClosedXML's Contains method for string values is typically case-insensitive by default.
                        filterColumn.Contains(FilterValue);
                        break;
                    case "startswith":
                        // ClosedXML's BeginsWith method for string values is typically case-insensitive by default.
                        filterColumn.BeginsWith(FilterValue);
                        break;
                    case "endswith":
                        // ClosedXML's EndsWith method for string values is typically case-insensitive by default.
                        filterColumn.EndsWith(FilterValue);
                        break;
                    case "greaterthan":
                        if (double.TryParse(FilterValue, out double gtValue))
                            filterColumn.GreaterThan(gtValue);
                        else
                            Console.WriteLine($"Warning: FilterValue '{FilterValue}' is not a valid number for GreaterThan comparison in column {FilterColumnIndex}.");
                        break;
                    case "lessthan":
                        if (double.TryParse(FilterValue, out double ltValue))
                            filterColumn.LessThan(ltValue);
                        else
                            Console.WriteLine($"Warning: FilterValue '{FilterValue}' is not a valid number for LessThan comparison in column {FilterColumnIndex}.");
                        break;
                    default:
                        // For any unrecognized operator, default to Equal
                        filterColumn.EqualTo(FilterValue);
                        Console.WriteLine($"Warning: Unrecognized filter operator '{FilterOperator}'. Defaulting to 'Equal' for column {FilterColumnIndex}.");
                        break;
                }

                workbook.Save();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during Excel filtering in FilterDataAction: {ex.Message}");
                return Task.FromException(ex);
            }
        }
    }
}