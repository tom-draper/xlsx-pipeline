using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class GroupRowsAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("fromRow")]
    public required int FromRow { get; set; }

    [JsonPropertyName("toRow")]
    public required int ToRow { get; set; }

    [JsonPropertyName("collapse")]
    public bool Collapse { get; set; } = false;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        worksheet.Rows(FromRow, ToRow).Group(Collapse);

        workbook.Save();
        return Task.CompletedTask;
    }
}
