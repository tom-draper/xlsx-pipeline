using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class MoveSheetAction : ActionBase
{
    // Backing fields
    private string? _sheetName;
    private string? _targetFilePath;

    /// <summary>
    /// Name of the sheet to move. Automatically processes date/time placeholders.
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
    /// Optional file path override. Automatically processes date/time placeholders.
    /// </summary>
    [JsonIgnore]
    public string? TargetFilePath
    {
        get => _targetFilePath != null ? Helpers.ReplaceDateTimePlaceholders(_targetFilePath) : null;
        set => _targetFilePath = value;
    }

    [JsonPropertyName("targetFilePath")]
    public string? JsonTargetFilePath
    {
        get => _targetFilePath;
        set => _targetFilePath = value;
    }

    /// <summary>
    /// 1-based index to move the sheet to. Defaults to 1 (first position).
    /// </summary>
    public int TargetIndex { get; set; } = 1;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (TargetFilePath == null || TargetFilePath == filePath)
                MoveSheetWithinSameWorkbook(filePath);
            else
                MoveSheetToDifferentWorkbook(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void MoveSheetWithinSameWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        Validation.ValidateSheetExists(workbook, SheetName);
        var worksheet = Helpers.GetWorksheet(workbook, SheetName);
        Validation.ValidateTargetIndex(workbook, TargetIndex);

        worksheet.Position = TargetIndex;
        workbook.Save();
    }

    private void MoveSheetToDifferentWorkbook(string filePath)
    {
        using var sourceWorkbook = new XLWorkbook(filePath);
        using var targetWorkbook = Helpers.GetOrCreateWorkbook(TargetFilePath!);

        var sourceWorksheet = Helpers.GetWorksheet(sourceWorkbook, SheetName);
        Validation.ValidateTargetIndex(targetWorkbook, TargetIndex);
        Validation.ValidateSheetExists(targetWorkbook, SheetName);

        var copiedWorksheet = sourceWorksheet.CopyTo(targetWorkbook);
        copiedWorksheet.Position = TargetIndex;

        sourceWorksheet.Delete();

        sourceWorkbook.Save();
        targetWorkbook.SaveAs(TargetFilePath!);
    }
}
