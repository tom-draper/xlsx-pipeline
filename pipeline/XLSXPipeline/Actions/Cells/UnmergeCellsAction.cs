using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Cells;

public class UnmergeCellsAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("range")]
    public required PlaceholderString Range { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        worksheet.Range((string)Range!).Unmerge();

        workbook.Save();
        return Task.CompletedTask;
    }
}
