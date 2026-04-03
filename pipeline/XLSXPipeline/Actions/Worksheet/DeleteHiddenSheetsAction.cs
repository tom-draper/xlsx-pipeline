using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class DeleteHiddenSheetsAction : ActionBase
{
    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var hiddenSheets = GetHiddenSheets(workbook);
        if (hiddenSheets.Count == 0)
            return Task.CompletedTask; // No hidden sheets to delete

        foreach (var sheet in hiddenSheets)
            sheet.Delete();

        workbook.Save();

        return Task.CompletedTask;
    }

    private static List<IXLWorksheet> GetHiddenSheets(XLWorkbook workbook)
    {
        return [.. workbook.Worksheets.Where(ws => ws.Visibility != XLWorksheetVisibility.Visible)];
    }
}
