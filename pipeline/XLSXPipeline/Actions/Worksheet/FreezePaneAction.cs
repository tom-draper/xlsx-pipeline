using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Worksheet;

public class FreezePaneAction : ActionBase
{
    /// <summary>
    /// Name of the sheet to apply the freeze to. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Number of rows to freeze from the top. 0 means no row freeze.
    /// </summary>
    public int Rows { get; set; } = 0;

    /// <summary>
    /// Number of columns to freeze from the left. 0 means no column freeze.
    /// </summary>
    public int Columns { get; set; } = 0;

    /// <summary>
    /// When true, removes any existing freeze pane before applying the new one.
    /// </summary>
    public bool UnfreezeFirst { get; set; } = true;

    protected override Task ExecuteInternalAsync(string filePath)
    {
        if (Rows < 0)
            throw new ArgumentOutOfRangeException(nameof(Rows), "Rows cannot be negative.");
        if (Columns < 0)
            throw new ArgumentOutOfRangeException(nameof(Columns), "Columns cannot be negative.");

        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);

        if (UnfreezeFirst)
            worksheet.SheetView.Freeze(0, 0);

        if (Rows > 0 || Columns > 0)
            worksheet.SheetView.Freeze(Rows, Columns);

        workbook.Save();
        return Task.CompletedTask;
    }
}
