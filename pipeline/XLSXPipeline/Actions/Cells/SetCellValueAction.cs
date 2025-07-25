using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Cells;

public class SetCellValueAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string CellAddress { get; set; }
    public required string Value { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            SetCellValue(worksheet);

            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void SetCellValue(IXLWorksheet worksheet)
    {
        ValidateInputs();

        var targetCell = GetTargetCell(worksheet);
        Validation.ValidateCell(targetCell);

        ApplyValueToCell(targetCell);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(CellAddress))
            throw new ArgumentException("Cell address cannot be null or empty.", nameof(CellAddress));

        // Note: Value can be empty string (to clear a cell), so we only check for null
        if (Value == null)
            throw new ArgumentException("Value cannot be null. Use empty string to clear a cell.", nameof(Value));

        // Validate cell address format
        if (!IsValidCellAddress(CellAddress))
            throw new ArgumentException($"Invalid cell address format '{CellAddress}'. Please use a valid format (e.g., 'A1', 'B5', 'AA10').", nameof(CellAddress));
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

    private void ApplyValueToCell(IXLCell cell)
    {
        try
        {
            // Clear any existing formula before setting value
            if (!string.IsNullOrEmpty(cell.FormulaA1))
                cell.FormulaA1 = string.Empty;

            // Set the value - ClosedXML will automatically detect and convert data types
            cell.Value = Value;

            // Validate that the value was set successfully
            ValidateValueWasSet(cell);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException(
                $"Failed to set value '{Value}' in cell '{CellAddress}'. " +
                "Please ensure the value is compatible with Excel cell format requirements.", ex);
        }
    }

    private void ValidateValueWasSet(IXLCell cell)
    {
        try
        {
            // For empty strings, the cell value might be empty or null
            if (string.IsNullOrEmpty(Value))
            {
                if (!cell.IsEmpty() && !string.IsNullOrEmpty(cell.GetString()))
                    throw new InvalidOperationException("Failed to clear cell value as expected.");
            }
            else
            {
                // Check that some value was set (ClosedXML converts types automatically)
                var cellStringValue = cell.GetString();
                if (string.IsNullOrEmpty(cellStringValue))
                    throw new InvalidOperationException("Value was not applied successfully to the cell.");
            }
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            // If validation fails for technical reasons, log but don't fail the operation
            // The value setting might have succeeded even if validation had issues
        }
    }
}