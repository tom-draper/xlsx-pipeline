using ClosedXML.Excel;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions.Data;

public class CopyColumnAction : ActionBase
{
    // Backing field for SheetName
    private string? _sheetName;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? SheetName
    {
        get => _sheetName != null ? Helpers.ReplaceDateTimePlaceholders(_sheetName) : null;
        set => _sheetName = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("sheetName")]
    public string? JsonSheetName
    {
        get => _sheetName;
        set => _sheetName = value;
    }
    public required string From { get; set; }
    public required string To { get; set; }
    public int Count { get; set; } = 1;
    // Backing field for TargetSheetName
    private string? _targetSheetName;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? TargetSheetName
    {
        get => _targetSheetName != null ? Helpers.ReplaceDateTimePlaceholders(_targetSheetName) : null;
        set => _targetSheetName = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("targetSheetName")]
    public string? JsonTargetSheetName
    {
        get => _targetSheetName;
        set => _targetSheetName = value;
    }
    // Backing field for TargetFilePath
    private string? _targetFilePath;

    /// <summary>
    /// Optional file path override. If provided, this will be used instead of the pipeline's current file path.
    /// Automatically processes date/time placeholders like {year}, {month}, {day}, etc.
    /// </summary>
    [JsonIgnore] // Don't serialize this computed property
    public string? TargetFilePath
    {
        get => _targetFilePath != null ? Helpers.ReplaceDateTimePlaceholders(_targetFilePath) : null;
        set => _targetFilePath = value;
    }

    /// <summary>
    /// JSON property that maps to the backing field for serialization/deserialization
    /// </summary>
    [JsonPropertyName("targetFilePath")]
    public string? JsonTargetFilePath
    {
        get => _targetFilePath;
        set => _targetFilePath = value;
    }
    public bool InsertColumns { get; set; } = false; // If true, insert new columns; if false, overwrite existing

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (TargetFilePath == null || TargetFilePath == filePath)
                CopyColumnWithinSameWorkbook(filePath);
            else
                CopyColumnToDifferentWorkbook(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    private void CopyColumnWithinSameWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var sourceSheet = Helpers.GetWorksheetOrFirst(workbook, SheetName);
        var targetSheet = Helpers.GetOrCreateWorksheet(workbook, TargetSheetName);

        CopyColumns(sourceSheet, targetSheet);

        workbook.Save();
    }

    private void CopyColumnToDifferentWorkbook(string filePath)
    {
        using var sourceWorkbook = new XLWorkbook(filePath);
        using var targetWorkbook = Helpers.GetOrCreateWorkbook(TargetFilePath!);

        var sourceSheet = Helpers.GetWorksheetOrFirst(sourceWorkbook, SheetName);
        var targetSheet = Helpers.GetOrCreateWorksheet(targetWorkbook, TargetSheetName);

        var sourceColumnNumber = GetColumnNumber(sourceSheet, From, "Source");
        var destColumnNumber = GetColumnNumber(targetSheet, To, "Destination");

        Validation.ValidateColumnRange(sourceColumnNumber, Count, "source");

        if (InsertColumns)
            InsertColumnsAtDestination(targetSheet, destColumnNumber);

        for (int i = 0; i < Count; i++)
        {
            var sourceCol = sourceSheet.Column(sourceColumnNumber + i);
            var destCol = targetSheet.Column(destColumnNumber + i);
            sourceCol.CopyTo(destCol);
        }

        targetWorkbook.SaveAs(TargetFilePath!);
    }

    private void CopyColumns(IXLWorksheet sourceSheet, IXLWorksheet destSheet)
    {
        var sourceColumnNumber = GetColumnNumber(sourceSheet, From, "Source");
        var destColumnNumber = GetColumnNumber(destSheet, To, "Destination");

        Validation.ValidateColumnRange(sourceColumnNumber, Count, "source");

        if (InsertColumns)
            InsertColumnsAtDestination(destSheet, destColumnNumber);

        CopyColumnRange(sourceSheet, destSheet, sourceColumnNumber, destColumnNumber);
    }

    private static int GetColumnNumber(IXLWorksheet sheet, string columnReference, string columnType)
    {
        try
        {
            return sheet.Column(columnReference).ColumnNumber();
        }
        catch
        {
            throw new InvalidOperationException($"{columnType} column '{columnReference}' is not valid.");
        }
    }

    private void InsertColumnsAtDestination(IXLWorksheet destSheet, int destColumnNumber)
    {
        if (Count > 1)
            destSheet.Column(destColumnNumber).InsertColumnsAfter(Count - 1);
    }

    private void CopyColumnRange(IXLWorksheet sourceSheet, IXLWorksheet destSheet, int sourceColumnNumber, int destColumnNumber)
    {
        for (int i = 0; i < Count; i++)
        {
            var sourceCol = sourceSheet.Column(sourceColumnNumber + i);
            var destCol = destSheet.Column(destColumnNumber + i);
            sourceCol.CopyTo(destCol);
        }
    }
}