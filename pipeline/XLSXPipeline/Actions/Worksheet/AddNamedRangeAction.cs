using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class AddNamedRangeAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("name")]
    public required PlaceholderString Name { get; set; }

    [JsonPropertyName("range")]
    public required PlaceholderString Range { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "Workbook";

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        if (Scope.Trim().Equals("Worksheet", StringComparison.OrdinalIgnoreCase))
            worksheet.DefinedNames.Add((string)Name!, worksheet.Range((string)Range!));
        else
            workbook.DefinedNames.Add((string)Name!, worksheet.Range((string)Range!));

        workbook.Save();
        return Task.CompletedTask;
    }
}
