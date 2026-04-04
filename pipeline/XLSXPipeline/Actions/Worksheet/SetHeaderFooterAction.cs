using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class SetHeaderFooterAction : ActionBase
{
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    [JsonPropertyName("leftHeader")]
    public PlaceholderString? LeftHeader { get; set; }

    [JsonPropertyName("centerHeader")]
    public PlaceholderString? CenterHeader { get; set; }

    [JsonPropertyName("rightHeader")]
    public PlaceholderString? RightHeader { get; set; }

    [JsonPropertyName("leftFooter")]
    public PlaceholderString? LeftFooter { get; set; }

    [JsonPropertyName("centerFooter")]
    public PlaceholderString? CenterFooter { get; set; }

    [JsonPropertyName("rightFooter")]
    public PlaceholderString? RightFooter { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        if (!string.IsNullOrEmpty(LeftHeader))   worksheet.PageSetup.Header.Left.AddText((string)LeftHeader!);
        if (!string.IsNullOrEmpty(CenterHeader)) worksheet.PageSetup.Header.Center.AddText((string)CenterHeader!);
        if (!string.IsNullOrEmpty(RightHeader))  worksheet.PageSetup.Header.Right.AddText((string)RightHeader!);
        if (!string.IsNullOrEmpty(LeftFooter))   worksheet.PageSetup.Footer.Left.AddText((string)LeftFooter!);
        if (!string.IsNullOrEmpty(CenterFooter)) worksheet.PageSetup.Footer.Center.AddText((string)CenterFooter!);
        if (!string.IsNullOrEmpty(RightFooter))  worksheet.PageSetup.Footer.Right.AddText((string)RightFooter!);

        workbook.Save();
        return Task.CompletedTask;
    }
}
