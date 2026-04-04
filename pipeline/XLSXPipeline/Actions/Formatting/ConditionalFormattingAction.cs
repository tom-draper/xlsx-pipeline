using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Formatting;

public enum ConditionalFormattingType
{
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual,
    Equal,
    NotEqual,
    Between,
    NotBetween,
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    IsBlank,
    IsNotBlank,
    IsError,
    IsDuplicate,
    IsUnique,
    IsTop,
    IsBottom,
    ColorScale,
    DataBar
}

public class ConditionalFormattingAction : ActionBase
{
    /// <summary>
    /// Name of the sheet. Defaults to the first sheet.
    /// </summary>
    [JsonPropertyName("sheetName")]
    public PlaceholderString? SheetName { get; set; }

    /// <summary>
    /// Cell range to apply the rule to, e.g. "A1:D100".
    /// </summary>
    public required string Range { get; set; }

    /// <summary>
    /// The condition type.
    /// </summary>
    public required ConditionalFormattingType Condition { get; set; }

    /// <summary>
    /// Comparison value (for most types). For IsTop/IsBottom this is the count (e.g. "10").
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Second value for Between and NotBetween.
    /// </summary>
    public string? Value2 { get; set; }

    /// <summary>
    /// Background fill color as hex (e.g. "#FF0000") or named color. Not used for ColorScale/DataBar.
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Font color as hex or named color. Not used for ColorScale/DataBar.
    /// </summary>
    public string? FontColor { get; set; }

    /// <summary>
    /// Whether to apply bold font. Not used for ColorScale/DataBar.
    /// </summary>
    public bool? Bold { get; set; }

    /// <summary>
    /// Whether to apply italic font. Not used for ColorScale/DataBar.
    /// </summary>
    public bool? Italic { get; set; }

    // ColorScale properties
    /// <summary>Color for the lowest value in a ColorScale rule.</summary>
    public string? MinColor { get; set; }

    /// <summary>Optional midpoint color for a three-color ColorScale rule.</summary>
    public string? MidColor { get; set; }

    /// <summary>Color for the highest value in a ColorScale rule.</summary>
    public string? MaxColor { get; set; }

    // DataBar property
    /// <summary>Bar fill color for a DataBar rule.</summary>
    public string? BarColor { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        var range = worksheet.Range(Range);
        var cf = range.AddConditionalFormat();

        ApplyCondition(cf);

        workbook.Save();
        return Task.CompletedTask;
    }

    private void ApplyCondition(IXLConditionalFormat cf)
    {
        switch (Condition)
        {
            case ConditionalFormattingType.ColorScale:
                ApplyColorScale(cf);
                return;

            case ConditionalFormattingType.DataBar:
                ApplyDataBar(cf);
                return;
        }

        var style = Condition switch
        {
            ConditionalFormattingType.GreaterThan    => cf.WhenGreaterThan(RequiredValue()),
            ConditionalFormattingType.LessThan       => cf.WhenLessThan(RequiredValue()),
            ConditionalFormattingType.GreaterOrEqual => cf.WhenEqualOrGreaterThan(RequiredValue()),
            ConditionalFormattingType.LessOrEqual    => cf.WhenEqualOrLessThan(RequiredValue()),
            ConditionalFormattingType.Equal          => cf.WhenEquals(RequiredValue()),
            ConditionalFormattingType.NotEqual       => cf.WhenNotEquals(RequiredValue()),
            ConditionalFormattingType.Between        => cf.WhenBetween(RequiredValue(), RequiredValue2()),
            ConditionalFormattingType.NotBetween     => cf.WhenNotBetween(RequiredValue(), RequiredValue2()),
            ConditionalFormattingType.Contains       => cf.WhenContains(RequiredValue()),
            ConditionalFormattingType.NotContains    => cf.WhenNotContains(RequiredValue()),
            ConditionalFormattingType.StartsWith     => cf.WhenStartsWith(RequiredValue()),
            ConditionalFormattingType.EndsWith       => cf.WhenEndsWith(RequiredValue()),
            ConditionalFormattingType.IsBlank        => cf.WhenIsBlank(),
            ConditionalFormattingType.IsNotBlank     => cf.WhenNotBlank(),
            ConditionalFormattingType.IsError        => cf.WhenIsError(),
            ConditionalFormattingType.IsDuplicate    => cf.WhenIsDuplicate(),
            ConditionalFormattingType.IsUnique       => cf.WhenIsUnique(),
            ConditionalFormattingType.IsTop          => cf.WhenIsTop(int.Parse(RequiredValue()), XLTopBottomType.Items),
            ConditionalFormattingType.IsBottom       => cf.WhenIsBottom(int.Parse(RequiredValue()), XLTopBottomType.Items),
            _ => throw new NotSupportedException($"Condition type '{Condition}' is not supported.")
        };

        ApplyStyle(style);
    }

    private void ApplyStyle(IXLStyle style)
    {
        if (!string.IsNullOrWhiteSpace(BackgroundColor))
            style.Fill.SetBackgroundColor(ParseColor(BackgroundColor));

        if (!string.IsNullOrWhiteSpace(FontColor))
            style.Font.SetFontColor(ParseColor(FontColor));

        if (Bold.HasValue)
            style.Font.SetBold(Bold.Value);

        if (Italic.HasValue)
            style.Font.SetItalic(Italic.Value);
    }

    private void ApplyColorScale(IXLConditionalFormat cf)
    {
        var minColor = ParseColor(MinColor ?? "#FF0000");
        var maxColor = ParseColor(MaxColor ?? "#00FF00");

        if (!string.IsNullOrWhiteSpace(MidColor))
        {
            cf.ColorScale()
                .LowestValue(minColor)
                .Midpoint(XLCFContentType.Percent, "50", ParseColor(MidColor))
                .HighestValue(maxColor);
        }
        else
        {
            cf.ColorScale()
                .LowestValue(minColor)
                .HighestValue(maxColor);
        }
    }

    private void ApplyDataBar(IXLConditionalFormat cf)
    {
        var color = ParseColor(BarColor ?? "#638EC6");
        cf.DataBar(color).LowestValue().HighestValue();
    }

    private string RequiredValue()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentException($"Value is required for condition type '{Condition}'.");
        return Value;
    }

    private string RequiredValue2()
    {
        if (string.IsNullOrWhiteSpace(Value2))
            throw new ArgumentException($"Value2 is required for condition type '{Condition}'.");
        return Value2;
    }

    private static XLColor ParseColor(string color)
    {
        try
        {
            return color.StartsWith('#') ? XLColor.FromHtml(color) : XLColor.FromName(color);
        }
        catch
        {
            throw new ArgumentException($"Invalid color value '{color}'.");
        }
    }
}
