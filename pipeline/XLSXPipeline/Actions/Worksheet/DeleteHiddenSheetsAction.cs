using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class DeleteHiddenSheetsAction : ActionBase
{
    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            // Find and delete all hidden sheets
            var hiddenSheets = workbook.Worksheets
                .Where(ws => ws.Visibility != XLWorksheetVisibility.Visible)
                .ToList(); // ToList() to avoid modifying during iteration

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
}
