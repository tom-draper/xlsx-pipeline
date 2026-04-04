using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class SetPageMarginsAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("left")]
    public double? Left { get; set; }

    [JsonPropertyName("right")]
    public double? Right { get; set; }

    [JsonPropertyName("top")]
    public double? Top { get; set; }

    [JsonPropertyName("bottom")]
    public double? Bottom { get; set; }

    [JsonPropertyName("header")]
    public double? Header { get; set; }

    [JsonPropertyName("footer")]
    public double? Footer { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        if (Left.HasValue) worksheet.PageSetup.Margins.Left = Left.Value;
        if (Right.HasValue) worksheet.PageSetup.Margins.Right = Right.Value;
        if (Top.HasValue) worksheet.PageSetup.Margins.Top = Top.Value;
        if (Bottom.HasValue) worksheet.PageSetup.Margins.Bottom = Bottom.Value;
        if (Header.HasValue) worksheet.PageSetup.Margins.Header = Header.Value;
        if (Footer.HasValue) worksheet.PageSetup.Margins.Footer = Footer.Value;

        workbook.Save();
        return Task.CompletedTask;
    }
}
