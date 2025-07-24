using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Advanced;

public class CreatePivotTableAction : ActionBase
{
    public string? SourceSheetName { get; set; }
    public required string SourceRange { get; set; }
    public required string DestinationSheetName { get; set; }
    public required string DestinationCell { get; set; }
    public List<string> RowFields { get; set; } = [];
    public List<string> ColumnFields { get; set; } = [];
    public List<string> DataFields { get; set; } = [];

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            CreatePivotTable(workbook);
            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void CreatePivotTable(XLWorkbook workbook)
    {
        ValidateInputs();

        var sourceWorksheet = GetSourceWorksheet(workbook);
        var sourceRange = GetSourceRange(sourceWorksheet);
        var destinationWorksheet = GetOrCreateDestinationWorksheet(workbook);
        var destinationCell = GetDestinationCell(destinationWorksheet);

        ValidateSourceData(sourceRange);
        ValidateFieldNames(sourceRange);

        var pivotTable = CreateAndConfigurePivotTable(destinationWorksheet, destinationCell, sourceRange);
        ConfigurePivotTableFields(pivotTable);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(SourceRange))
            throw new ArgumentException("Source range cannot be null or empty.", nameof(SourceRange));

        if (string.IsNullOrWhiteSpace(DestinationSheetName))
            throw new ArgumentException("Destination sheet name cannot be null or empty.", nameof(DestinationSheetName));

        if (string.IsNullOrWhiteSpace(DestinationCell))
            throw new ArgumentException("Destination cell cannot be null or empty.", nameof(DestinationCell));

        // Validate that at least one field is specified
        if (!RowFields.Any() && !ColumnFields.Any() && !DataFields.Any())
            throw new ArgumentException("At least one field must be specified (RowFields, ColumnFields, or DataFields).");

        // Validate cell address format
        if (!IsValidCellAddress(DestinationCell))
            throw new ArgumentException($"Invalid destination cell address format '{DestinationCell}'. Please use a valid format (e.g., 'A1', 'B5').", nameof(DestinationCell));
    }

    private static bool IsValidCellAddress(string cellAddress)
    {
        try
        {
            return !string.IsNullOrWhiteSpace(cellAddress) &&
                   cellAddress.Length >= 2 &&
                   char.IsLetter(cellAddress[0]);
        }
        catch
        {
            return false;
        }
    }

    private IXLWorksheet GetSourceWorksheet(XLWorkbook workbook)
    {
        try
        {
            if (string.IsNullOrEmpty(SourceSheetName))
            {
                if (workbook.Worksheets.Count == 0)
                    throw new InvalidOperationException("No worksheets found in the workbook.");
                return workbook.Worksheets.First();
            }

            var worksheet = workbook.Worksheet(SourceSheetName);
            if (worksheet == null)
                throw new InvalidOperationException($"Source worksheet '{SourceSheetName}' does not exist.");

            return worksheet;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Failed to retrieve source worksheet '{SourceSheetName ?? "first worksheet"}'.", ex);
        }
    }

    private IXLRange GetSourceRange(IXLWorksheet sourceWorksheet)
    {
        try
        {
            var range = sourceWorksheet.Range(SourceRange);
            if (range == null)
                throw new InvalidOperationException($"Failed to create range from '{SourceRange}'.");

            return range;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid source range specification '{SourceRange}'. Please ensure the range is valid (e.g., 'A1:D100').", ex);
        }
    }

    private IXLWorksheet GetOrCreateDestinationWorksheet(XLWorkbook workbook)
    {
        try
        {
            var worksheet = workbook.Worksheet(DestinationSheetName);
            if (worksheet != null)
                return worksheet;

            // Create new worksheet if it doesn't exist
            return workbook.Worksheets.Add(DestinationSheetName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get or create destination worksheet '{DestinationSheetName}'.", ex);
        }
    }

    private IXLCell GetDestinationCell(IXLWorksheet destinationWorksheet)
    {
        try
        {
            return destinationWorksheet.Cell(DestinationCell);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to locate destination cell '{DestinationCell}' in worksheet '{DestinationSheetName}'.", ex);
        }
    }

    private static void ValidateSourceData(IXLRange sourceRange)
    {
        if (sourceRange.IsEmpty())
            throw new InvalidOperationException("Source range is empty. Pivot tables require data to analyze.");

        if (sourceRange.RowCount() < 2)
            throw new InvalidOperationException("Source range must contain at least 2 rows (header row and data row) for pivot table creation.");

        if (sourceRange.ColumnCount() < 1)
            throw new InvalidOperationException("Source range must contain at least 1 column for pivot table creation.");
    }

    private void ValidateFieldNames(IXLRange sourceRange)
    {
        try
        {
            var headerRow = sourceRange.FirstRow();
            var availableFields = GetAvailableFieldNames(headerRow);

            ValidateFieldList(RowFields, availableFields, nameof(RowFields));
            ValidateFieldList(ColumnFields, availableFields, nameof(ColumnFields));
            ValidateFieldList(DataFields, availableFields, nameof(DataFields));
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new InvalidOperationException("Failed to validate field names against source data headers.", ex);
        }
    }

    private static List<string> GetAvailableFieldNames(IXLRangeRow headerRow)
    {
        var fieldNames = new List<string>();

        foreach (var cell in headerRow.Cells())
        {
            var fieldName = cell.GetString().Trim();
            if (!string.IsNullOrEmpty(fieldName))
                fieldNames.Add(fieldName);
        }

        if (fieldNames.Count == 0)
            throw new InvalidOperationException("No valid field names found in the header row. Ensure the first row contains column headers.");

        return fieldNames;
    }

    private static void ValidateFieldList(List<string> fields, List<string> availableFields, string fieldType)
    {
        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException($"Empty or null field name found in {fieldType}.", fieldType);

            if (!availableFields.Contains(field, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException(
                    $"Field '{field}' in {fieldType} was not found in source data headers. " +
                    $"Available fields: {string.Join(", ", availableFields)}", fieldType);
        }
    }

    private IXLPivotTable CreateAndConfigurePivotTable(IXLWorksheet destinationWorksheet, IXLCell destinationCell, IXLRange sourceRange)
    {
        try
        {
            var pivotTableName = GenerateUniquePivotTableName(destinationWorksheet);
            return destinationWorksheet.PivotTables.Add(pivotTableName, destinationCell, sourceRange);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create pivot table at cell '{DestinationCell}' in worksheet '{DestinationSheetName}'. " +
                "Ensure the destination cell is not occupied and there's sufficient space.", ex);
        }
    }

    private static string GenerateUniquePivotTableName(IXLWorksheet worksheet)
    {
        var baseName = "PivotTable";
        var counter = 1;

        while (worksheet.PivotTables.Any(pt => pt.Name.Equals($"{baseName}{counter}", StringComparison.OrdinalIgnoreCase)))
        {
            counter++;
        }

        return $"{baseName}{counter}";
    }

    private void ConfigurePivotTableFields(IXLPivotTable pivotTable)
    {
        try
        {
            AddFieldsToPivotTable(pivotTable);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to configure pivot table fields.", ex);
        }
    }

    private void AddFieldsToPivotTable(IXLPivotTable pivotTable)
    {
        // Add row fields
        foreach (var field in RowFields)
        {
            try
            {
                pivotTable.RowLabels.Add(field);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add row field '{field}' to pivot table.", ex);
            }
        }

        // Add column fields
        foreach (var field in ColumnFields)
        {
            try
            {
                pivotTable.ColumnLabels.Add(field);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add column field '{field}' to pivot table.", ex);
            }
        }

        // Add data fields
        foreach (var field in DataFields)
        {
            try
            {
                pivotTable.Values.Add(field);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add data field '{field}' to pivot table.", ex);
            }
        }
    }
}