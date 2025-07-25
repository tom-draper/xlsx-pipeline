using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class UnprotectSheetAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string Password { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet =  Helpers.GetWorksheetOrFirst(workbook, SheetName);
            Validation.ValidatePassword(Password);

            if (worksheet.Protection.IsProtected)
            {
                worksheet.Unprotect(Password);
                workbook.Save();
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
