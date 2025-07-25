using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class MoveSheetAction : ActionBase
{
    public required string SheetName { get; set; }
    public int TargetIndex { get; set; } = 1; // 1-based index, defaults to first position
    public string? TargetFilePath { get; set; }

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

        // Copy the sheet to target workbook
        var copiedWorksheet = sourceWorksheet.CopyTo(targetWorkbook);
        copiedWorksheet.Position = TargetIndex;

        // Remove from source workbook
        sourceWorksheet.Delete();

        // Save both workbooks
        sourceWorkbook.Save();
        targetWorkbook.SaveAs(TargetFilePath!);
    }
}