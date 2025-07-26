using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class CopySheetAction : ActionBase
{
    // Backing fields
    private string? _sourceSheetName;
    private string? _newSheetName;
    private string? _targetFilePath;

    /// <summary>
    /// Name of the sheet to copy.
    /// </summary>
    [JsonIgnore]
    public string? SourceSheetName
    {
        get => _sourceSheetName != null ? Helpers.ReplaceDateTimePlaceholders(_sourceSheetName) : null;
        set => _sourceSheetName = value;
    }

    /// <summary>
    /// New name for the copied sheet.
    /// </summary>
    [JsonIgnore]
    public string? NewSheetName
    {
        get => _newSheetName != null ? Helpers.ReplaceDateTimePlaceholders(_newSheetName) : null;
        set => _newSheetName = value;
    }

    /// <summary>
    /// Optional path to a different workbook where the sheet should be copied.
    /// </summary>
    [JsonIgnore]
    public string? TargetFilePath
    {
        get => _targetFilePath != null ? Helpers.ReplaceDateTimePlaceholders(_targetFilePath) : null;
        set => _targetFilePath = value;
    }

    // JSON mapping properties
    [JsonPropertyName("sourceSheetName")]
    public string? JsonSourceSheetName
    {
        get => _sourceSheetName;
        set => _sourceSheetName = value;
    }

    [JsonPropertyName("newSheetName")]
    public string? JsonNewSheetName
    {
        get => _newSheetName;
        set => _newSheetName = value;
    }

    [JsonPropertyName("targetFilePath")]
    public string? JsonTargetFilePath
    {
        get => _targetFilePath;
        set => _targetFilePath = value;
    }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (TargetFilePath == null || TargetFilePath == filePath)
                CopySheetWithinSameWorkbook(filePath);
            else
                CopySheetToDifferentWorkbook(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void CopySheetWithinSameWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        Validation.ValidateSheetExists(workbook, SourceSheetName);
        Validation.ValidateSheetNotExists(workbook, NewSheetName);

        var worksheet = Helpers.GetWorksheet(workbook, SourceSheetName);
        worksheet.CopyTo(NewSheetName);
        workbook.Save();
    }

    private void CopySheetToDifferentWorkbook(string filePath)
    {
        using var sourceWorkbook = new XLWorkbook(filePath);
        using var targetWorkbook = Helpers.GetOrCreateWorkbook(TargetFilePath!);

        Validation.ValidateSheetExists(sourceWorkbook, SourceSheetName);
        Validation.ValidateSheetNotExists(targetWorkbook, NewSheetName);

        var sourceWorksheet = Helpers.GetWorksheet(sourceWorkbook, SourceSheetName);
        sourceWorksheet.CopyTo(targetWorkbook, NewSheetName);
        targetWorkbook.SaveAs(TargetFilePath!);
    }
}
