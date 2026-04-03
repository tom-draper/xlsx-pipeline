using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class UnprotectSheetAction : ActionBase
{
    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    public required string Password { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
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
}
