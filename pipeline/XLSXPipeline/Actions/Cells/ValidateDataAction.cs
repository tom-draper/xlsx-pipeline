using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Cells;

public class ValidateDataAction : ActionBase
{
    // Backing field for SheetName
    private string? _sheetName;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? SheetName
    {
        get => _sheetName != null ? Helpers.ReplaceDateTimePlaceholders(_sheetName) : null;
        set => _sheetName = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("sheetName")]
    public string? JsonSheetName
    {
        get => _sheetName;
        set => _sheetName = value;
    }
    public required string Range { get; set; }
    public string ValidationType { get; set; } = "List";
    public required string ValidationCriteria { get; set; }
    public string? ErrorMessage { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = GetWorksheet(workbook, SheetName);
        ApplyDataValidation(worksheet);
        workbook.Save();
        return Task.CompletedTask;
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string? sheetName)
    {
        if (string.IsNullOrEmpty(sheetName))
        {
            if (workbook.Worksheets.Count == 0)
                throw new InvalidOperationException("No worksheets found in the workbook.");
            return workbook.Worksheets.First();
        }

        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Worksheet '{sheetName}' does not exist.");

        return worksheet;
    }

    private void ApplyDataValidation(IXLWorksheet worksheet)
    {
        ValidateInputs();

        var targetRange = GetTargetRange(worksheet);
        ValidateRange(targetRange);

        ConfigureDataValidation(targetRange);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(Range))
            throw new ArgumentException("Range cannot be null or empty.", nameof(Range));

        if (string.IsNullOrWhiteSpace(ValidationType))
            throw new ArgumentException("Validation type cannot be null or empty.", nameof(ValidationType));

        if (string.IsNullOrWhiteSpace(ValidationCriteria))
            throw new ArgumentException("Validation criteria cannot be null or empty.", nameof(ValidationCriteria));

        ValidateValidationType();
        ValidateValidationCriteria();
    }

    private void ValidateValidationType()
    {
        var validTypes = new[] { "list", "whole", "decimal" };
        var normalizedType = ValidationType.ToLowerInvariant();

        if (!validTypes.Contains(normalizedType))
            throw new ArgumentException(
                $"Invalid validation type '{ValidationType}'. Supported types are: {string.Join(", ", validTypes)}.",
                nameof(ValidationType));
    }

    private void ValidateValidationCriteria()
    {
        var normalizedType = ValidationType.ToLowerInvariant();

        switch (normalizedType)
        {
            case "list":
                ValidateListCriteria();
                break;
            case "whole":
                ValidateWholeCriteria();
                break;
            case "decimal":
                ValidateDecimalCriteria();
                break;
        }
    }

    private void ValidateListCriteria()
    {
        if (string.IsNullOrWhiteSpace(ValidationCriteria))
            throw new ArgumentException("List validation criteria cannot be empty.", nameof(ValidationCriteria));

        // For list validation, criteria should be a comma-separated list or range reference
        // Basic validation - more complex validation will be handled by ClosedXML
    }

    private void ValidateWholeCriteria()
    {
        var parts = ValidationCriteria.Split(',');
        if (parts.Length != 2)
            throw new ArgumentException(
                "Whole number validation criteria must contain exactly two comma-separated values (min,max).",
                nameof(ValidationCriteria));

        if (!int.TryParse(parts[0].Trim(), out var min))
            throw new ArgumentException(
                $"Invalid minimum value '{parts[0].Trim()}' for whole number validation. Must be a valid integer.",
                nameof(ValidationCriteria));

        if (!int.TryParse(parts[1].Trim(), out var max))
            throw new ArgumentException(
                $"Invalid maximum value '{parts[1].Trim()}' for whole number validation. Must be a valid integer.",
                nameof(ValidationCriteria));

        if (min >= max)
            throw new ArgumentException(
                $"Minimum value ({min}) must be less than maximum value ({max}) for range validation.",
                nameof(ValidationCriteria));
    }

    private void ValidateDecimalCriteria()
    {
        var parts = ValidationCriteria.Split(',');
        if (parts.Length != 2)
            throw new ArgumentException(
                "Decimal validation criteria must contain exactly two comma-separated values (min,max).",
                nameof(ValidationCriteria));

        if (!double.TryParse(parts[0].Trim(), out var min))
            throw new ArgumentException(
                $"Invalid minimum value '{parts[0].Trim()}' for decimal validation. Must be a valid number.",
                nameof(ValidationCriteria));

        if (!double.TryParse(parts[1].Trim(), out var max))
            throw new ArgumentException(
                $"Invalid maximum value '{parts[1].Trim()}' for decimal validation. Must be a valid number.",
                nameof(ValidationCriteria));

        if (min >= max)
            throw new ArgumentException(
                $"Minimum value ({min}) must be less than maximum value ({max}) for range validation.",
                nameof(ValidationCriteria));
    }

    private IXLRange GetTargetRange(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Range(Range);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid range specification '{Range}'. Please ensure the range is valid (e.g., 'A1:C10').", ex);
        }
    }

    private static void ValidateRange(IXLRange range)
    {
        if (range == null)
            throw new InvalidOperationException("Target range could not be retrieved from the worksheet.");

        if (!range.Cells().Any())
            throw new InvalidOperationException("The specified range contains no cells.");
    }

    private void ConfigureDataValidation(IXLRange range)
    {
        try
        {
            var validation = range.GetDataValidation();
            ApplyValidationRule(validation);

            if (!string.IsNullOrWhiteSpace(ErrorMessage))
                SetErrorMessage(validation);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to configure data validation for range '{Range}' with type '{ValidationType}'.", ex);
        }
    }

    private void ApplyValidationRule(IXLDataValidation validation)
    {
        var normalizedType = ValidationType.ToLowerInvariant();

        switch (normalizedType)
        {
            case "list":
                ApplyListValidation(validation);
                break;
            case "whole":
                ApplyWholeNumberValidation(validation);
                break;
            case "decimal":
                ApplyDecimalValidation(validation);
                break;
            default:
                throw new InvalidOperationException($"Unsupported validation type: {ValidationType}");
        }
    }

    private void ApplyListValidation(IXLDataValidation validation)
    {
        try
        {
            validation.List(ValidationCriteria);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to apply list validation with criteria '{ValidationCriteria}'. " +
                "Ensure the criteria is a valid comma-separated list or range reference.", ex);
        }
    }

    private void ApplyWholeNumberValidation(IXLDataValidation validation)
    {
        try
        {
            var parts = ValidationCriteria.Split(',');
            var min = int.Parse(parts[0].Trim());
            var max = int.Parse(parts[1].Trim());

            validation.WholeNumber.Between(min, max);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to apply whole number validation with criteria '{ValidationCriteria}'.", ex);
        }
    }

    private void ApplyDecimalValidation(IXLDataValidation validation)
    {
        try
        {
            var parts = ValidationCriteria.Split(',');
            var min = double.Parse(parts[0].Trim());
            var max = double.Parse(parts[1].Trim());

            validation.Decimal.Between(min, max);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to apply decimal validation with criteria '{ValidationCriteria}'.", ex);
        }
    }

    private void SetErrorMessage(IXLDataValidation validation)
    {
        try
        {
            validation.ErrorMessage = ErrorMessage;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set error message '{ErrorMessage}' for data validation.", ex);
        }
    }
}