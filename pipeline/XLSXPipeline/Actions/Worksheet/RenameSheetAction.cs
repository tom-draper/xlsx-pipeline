using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class RenameSheetAction : ActionBase
{
    // Backing fields
    private string? _sheetName;
    private string? _newSheetName;

    /// <summary>
    /// Sheet name to rename. Automatically processes date/time placeholders.
    /// </summary>
    [JsonIgnore]
    public string? SheetName
    {
        get => _sheetName != null ? Helpers.ReplaceDateTimePlaceholders(_sheetName) : null;
        set => _sheetName = value;
    }

    [JsonPropertyName("sheetName")]
    public string? JsonSheetName
    {
        get => _sheetName;
        set => _sheetName = value;
    }

    /// <summary>
    /// New name for the sheet. Automatically processes date/time placeholders.
    /// </summary>
    [JsonIgnore]
    public string? NewSheetName
    {
        get => _newSheetName != null ? Helpers.ReplaceDateTimePlaceholders(_newSheetName) : null;
        set => _newSheetName = value;
    }

    [JsonPropertyName("newSheetName")]
    public string? JsonNewSheetName
    {
        get => _newSheetName;
        set => _newSheetName = value;
    }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            Validation.ValidateSheetExists(workbook, SheetName);
            var worksheet = Helpers.GetWorksheet(workbook, SheetName);
            Validation.ValidateSheetNotExists(workbook, NewSheetName);

            worksheet.Name = NewSheetName;
            workbook.Save();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
