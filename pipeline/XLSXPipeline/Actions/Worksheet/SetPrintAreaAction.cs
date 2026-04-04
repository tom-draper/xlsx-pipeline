using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class SetPrintAreaAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("range")]
    public PlaceholderString? Range { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        if (string.IsNullOrEmpty(Range))
            worksheet.PageSetup.PrintAreas.Clear();
        else
            worksheet.PageSetup.PrintAreas.Add((string)Range!);

        workbook.Save();
        return Task.CompletedTask;
    }
}
