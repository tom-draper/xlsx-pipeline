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

        var worksheet = GetWorksheet(workbook, SheetName);
        ValidateTargetIndex(workbook, TargetIndex);

        worksheet.Position = TargetIndex;
        workbook.Save();
    }

    private void MoveSheetToDifferentWorkbook(string filePath)
    {
        using var sourceWorkbook = new XLWorkbook(filePath);
        using var targetWorkbook = LoadOrCreateTargetWorkbook(TargetFilePath!);

        var sourceWorksheet = GetWorksheet(sourceWorkbook, SheetName);
        ValidateTargetIndex(targetWorkbook, TargetIndex);
        ValidateSheetName(targetWorkbook, SheetName);

        // Copy the sheet to target workbook
        var copiedWorksheet = sourceWorksheet.CopyTo(targetWorkbook);
        copiedWorksheet.Position = TargetIndex;

        // Remove from source workbook
        sourceWorksheet.Delete();

        // Save both workbooks
        sourceWorkbook.Save();
        targetWorkbook.SaveAs(TargetFilePath!);
    }

    private static XLWorkbook LoadOrCreateTargetWorkbook(string targetFilePath)
    {
        return System.IO.File.Exists(targetFilePath)
            ? new XLWorkbook(targetFilePath)
            : new XLWorkbook();
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName)
    {
        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Sheet '{sheetName}' does not exist.");
        return worksheet;
    }

    private static void ValidateTargetIndex(XLWorkbook workbook, int targetIndex)
    {
        int maxIndex = workbook.Worksheets.Count + 1; // +1 because we might be adding a sheet
        if (targetIndex < 1 || targetIndex > maxIndex)
            throw new ArgumentOutOfRangeException(nameof(targetIndex),
                $"Target index must be between 1 and {maxIndex}.");
    }

    private static void ValidateSheetName(XLWorkbook workbook, string sheetName)
    {
        if (workbook.Worksheets.Any(ws => ws.Name == sheetName))
            throw new InvalidOperationException($"Sheet '{sheetName}' already exists in the target workbook.");
    }
}