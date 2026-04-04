using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class CopySheetAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to copy.
    /// </summary>
    [JsonPropertyName("sourceSheetName")]
    public PlaceholderString? SourceSheetName { get; set; }

    /// <summary>
    /// New name for the copied sheet.
    /// </summary>
    [JsonPropertyName("newSheetName")]
    public PlaceholderString? NewSheetName { get; set; }

    /// <summary>
    /// Optional path to a different workbook where the sheet should be copied.
    /// </summary>
    [JsonPropertyName("targetFilePath")]
    public PlaceholderString? TargetFilePath { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        if (TargetFilePath == null || TargetFilePath == filePath)
            CopySheetWithinSameWorkbook(filePath);
        else
            CopySheetToDifferentWorkbook(filePath);

        return Task.CompletedTask;
    }

    private void CopySheetWithinSameWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        Validation.ValidateSheetExists(workbook, SourceSheetName);
        Validation.ValidateSheetNotExists(workbook, NewSheetName);

        var worksheet = Helpers.GetWorksheet(workbook, SourceSheetName);
        worksheet.CopyTo(NewSheetName!);
        workbook.Save();
    }

    private void CopySheetToDifferentWorkbook(string filePath)
    {
        using var sourceWorkbook = new XLWorkbook(filePath);
        using var targetWorkbook = Helpers.GetOrCreateWorkbook(TargetFilePath!);

        Validation.ValidateSheetExists(sourceWorkbook, SourceSheetName);
        Validation.ValidateSheetNotExists(targetWorkbook, NewSheetName);

        var sourceWorksheet = Helpers.GetWorksheet(sourceWorkbook, SourceSheetName);
        sourceWorksheet.CopyTo(targetWorkbook, NewSheetName!);
        targetWorkbook.SaveAs(TargetFilePath!);
    }
}
