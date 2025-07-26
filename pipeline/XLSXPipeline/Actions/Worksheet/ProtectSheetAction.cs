using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class ProtectSheetAction : ActionBase
{
    [ReplacePlaceholders]
    public string? SheetName { get; set; }
    public required string Password { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
            Validation.ValidatePassword(Password);

            var protection = worksheet.Protect(Password);

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

}