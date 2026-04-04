using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class GroupColumnsAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("fromColumn")]
    public required string FromColumn { get; set; }

    [JsonPropertyName("toColumn")]
    public required string ToColumn { get; set; }

    [JsonPropertyName("collapse")]
    public bool Collapse { get; set; } = false;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        int fromColNum = Helpers.GetColumnNumberFromLetter(FromColumn);
        int toColNum = Helpers.GetColumnNumberFromLetter(ToColumn);

        worksheet.Columns(fromColNum, toColNum).Group(Collapse);

        workbook.Save();
        return Task.CompletedTask;
    }
}
