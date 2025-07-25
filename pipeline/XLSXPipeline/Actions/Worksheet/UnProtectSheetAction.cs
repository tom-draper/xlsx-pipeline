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

            var worksheet =  GetWorksheet(workbook, SheetName);
            ValidatePassword(Password);

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

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string? sheetName)
    {
        var worksheet = string.IsNullOrEmpty(sheetName)
                ? workbook.Worksheets.First()
                : workbook.Worksheet(sheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Sheet '{sheetName}' does not exist.");
        return worksheet;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
    }
}
