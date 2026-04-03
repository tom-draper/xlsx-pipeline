using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class ReplaceSheetAction : ActionBase
{
    /// <summary>
    /// Sheet name in the target workbook to be replaced.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonPropertyName("targetSheetName")]
    public PlaceholderString? TargetSheetName { get; set; }

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonPropertyName("sourceFilePath")]
    public PlaceholderString? SourceFilePath { get; set; }

    /// <summary>
    /// Sheet name in the source workbook to copy from.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonPropertyName("sourceSheetName")]
    public PlaceholderString? SourceSheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        if (SourceFilePath == null)
            ReplaceSheetWithinSameWorkbook(filePath);
        else
            ReplaceSheetFromDifferentWorkbook(filePath);

        return Task.CompletedTask;
    }

    private void ReplaceSheetWithinSameWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        Validation.ValidateSheetExists(workbook, SourceSheetName);
        Validation.ValidateSheetExists(workbook, TargetSheetName);
        var sourceWorksheet = Helpers.GetWorksheet(workbook, SourceSheetName);
        var targetWorksheet = Helpers.GetWorksheet(workbook, TargetSheetName);

        int targetPosition = targetWorksheet.Position;
        targetWorksheet.Delete();

        var copiedWorksheet = CopyAndRenameWorksheet(sourceWorksheet, workbook, TargetSheetName!, targetPosition);

        workbook.Save();
    }

    private void ReplaceSheetFromDifferentWorkbook(string filePath)
    {
        using var targetWorkbook = new XLWorkbook(filePath);
        using var sourceWorkbook = new XLWorkbook(SourceFilePath);

        Validation.ValidateSheetExists(sourceWorkbook, SourceSheetName);
        Validation.ValidateSheetExists(targetWorkbook, TargetSheetName);
        var sourceWorksheet = Helpers.GetWorksheet(sourceWorkbook, SourceSheetName);
        var targetWorksheet = Helpers.GetWorksheet(targetWorkbook, TargetSheetName);

        int targetPosition = targetWorksheet.Position;
        targetWorksheet.Delete();

        var copiedWorksheet = CopyAndRenameWorksheet(sourceWorksheet, targetWorkbook, TargetSheetName!, targetPosition);

        targetWorkbook.Save();
    }

    private static IXLWorksheet CopyAndRenameWorksheet(IXLWorksheet sourceWorksheet, XLWorkbook targetWorkbook, string newName, int position)
    {
        var copiedWorksheet = sourceWorksheet.CopyTo(targetWorkbook);
        copiedWorksheet.Name = newName;
        copiedWorksheet.Position = position;
        return copiedWorksheet;
    }
}
