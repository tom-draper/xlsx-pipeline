using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Formatting;

public class FormatCellsAction : ActionBase
{
    public string? SheetName { get; set; }
    public required string Range { get; set; }
    public string? NumberFormat { get; set; }
    public string? FontName { get; set; }
    public int FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public string? BackgroundColor { get; set; }
    public string? FontColor { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        var range = worksheet.Range(Range);

        ApplyFormatting(range);

        workbook.Save();

        return Task.CompletedTask;
    }

    private void ApplyFormatting(IXLRange range)
    {
        ApplyNumberFormat(range);
        ApplyFontFormatting(range);
        ApplyColorFormatting(range);
    }

    private void ApplyNumberFormat(IXLRange range)
    {
        if (!string.IsNullOrEmpty(NumberFormat))
            range.Style.NumberFormat.Format = NumberFormat;
    }

    private void ApplyFontFormatting(IXLRange range)
    {
        var font = range.Style.Font;

        if (!string.IsNullOrEmpty(FontName))
            font.FontName = FontName;

        if (FontSize > 0)
            font.FontSize = FontSize;

        if (Bold)
            font.Bold = true;

        if (Italic)
            font.Italic = true;

        if (!string.IsNullOrEmpty(FontColor))
            font.FontColor = XLColor.FromName(FontColor);
    }

    private void ApplyColorFormatting(IXLRange range)
    {
        if (!string.IsNullOrEmpty(BackgroundColor))
            range.Style.Fill.BackgroundColor = XLColor.FromName(BackgroundColor);
    }
}