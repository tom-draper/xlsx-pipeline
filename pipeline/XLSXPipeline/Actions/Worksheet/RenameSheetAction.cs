using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class RenameSheetAction : ActionBase
{
    [ReplacePlaceholders]
    public required string SheetName { get; set; }
    [ReplacePlaceholders]
    public required string NewSheetName { get; set; }

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