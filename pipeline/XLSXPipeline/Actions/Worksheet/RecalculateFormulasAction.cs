using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class RecalculateFormulasAction : ActionBase
{
    /// <summary>
    /// Optional sheet name. If provided, only that sheet is targeted; otherwise all sheets are targeted.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        if (!string.IsNullOrEmpty(SheetName))
        {
            var worksheet = Helpers.GetWorksheet(workbook, SheetName);
            worksheet.RecalculateAllFormulas();
        }
        else
        {
            workbook.RecalculateAllFormulas();
        }

        workbook.Save();
        return Task.CompletedTask;
    }
}
