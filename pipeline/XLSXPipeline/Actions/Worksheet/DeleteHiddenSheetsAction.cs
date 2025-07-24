using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class DeleteHiddenSheetsAction : ActionBase
{
    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
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
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private static List<IXLWorksheet> GetHiddenSheets(XLWorkbook workbook)
    {
        return [.. workbook.Worksheets.Where(ws => ws.Visibility != XLWorksheetVisibility.Visible)];
    }
}
