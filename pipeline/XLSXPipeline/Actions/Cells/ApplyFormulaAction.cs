using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Cells;

public class ApplyFormulaAction : ActionBase
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
    public required string CellAddress { get; set; }
    public required string Formula { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            ApplyFormula(worksheet);
            
            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void ApplyFormula(IXLWorksheet worksheet)
    {
        ValidateInputs();
        
        var targetCell = GetTargetCell(worksheet);
        Validation.ValidateCell(targetCell);
        
        SetCellFormula(targetCell);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(CellAddress))
            throw new ArgumentException("Cell address cannot be null or empty.", nameof(CellAddress));

        if (string.IsNullOrWhiteSpace(Formula))
            throw new ArgumentException("Formula cannot be null or empty.", nameof(Formula));

        // Validate cell address format
        if (!IsValidCellAddress(CellAddress))
            throw new ArgumentException($"Invalid cell address format '{CellAddress}'. Please use a valid format (e.g., 'A1', 'B5', 'AA10').", nameof(CellAddress));

        // Validate formula format (should start with =)
        ValidateFormulaFormat();
    }

    private void ValidateFormulaFormat()
    {
        var trimmedFormula = Formula.Trim();
        
        if (string.IsNullOrEmpty(trimmedFormula))
            throw new ArgumentException("Formula cannot be empty after trimming whitespace.", nameof(Formula));

        // Formula should start with = (Excel convention)
        if (!trimmedFormula.StartsWith('='))
            throw new ArgumentException($"Formula '{Formula}' must start with '=' character. Example: '=SUM(A1:A10)'.", nameof(Formula));

        // Check for obviously invalid formulas (basic validation)
        if (trimmedFormula.Length == 1) // Just "="
            throw new ArgumentException("Formula cannot be just '=' character. Please provide a complete formula.", nameof(Formula));
    }

    private static bool IsValidCellAddress(string cellAddress)
    {
        try
        {
            // Simple validation - try to access the cell address
            // ClosedXML will validate the format when we try to use it
            return !string.IsNullOrWhiteSpace(cellAddress) &&
                   cellAddress.Length >= 2 &&
                   char.IsLetter(cellAddress[0]);
        }
        catch
        {
            return false;
        }
    }

    private IXLCell GetTargetCell(IXLWorksheet worksheet)
    {
        try
        {
            return worksheet.Cell(CellAddress);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to locate cell at address '{CellAddress}'. Please ensure the cell address is valid.", ex);
        }
    }

    private void SetCellFormula(IXLCell cell)
    {
        try
        {
            var trimmedFormula = Formula.Trim();
            cell.FormulaA1 = trimmedFormula;
            
            // Optionally validate that the formula was set correctly
            if (string.IsNullOrEmpty(cell.FormulaA1))
                throw new InvalidOperationException("Formula was not applied successfully to the cell.");
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException(
                $"Failed to apply formula '{Formula}' to cell '{CellAddress}'. " +
                "Please check that the formula syntax is correct and all referenced cells/ranges are valid.", ex);
        }
    }
}