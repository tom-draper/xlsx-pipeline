using XLSXPipeline.Actions;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Actions.Time;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Actions.Advanced;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Services;

public class ActionJsonConverter : JsonConverter<ActionBase>
{
    public override ActionBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var rootElement = jsonDoc.RootElement;

        if (!rootElement.TryGetProperty("type", out var typeProperty))
            throw new JsonException("Missing required 'type' property in action JSON.");

        var actionType = typeProperty.GetString();
        if (string.IsNullOrEmpty(actionType))
            throw new JsonException("Action 'type' property cannot be null or empty.");

        var rawText = rootElement.GetRawText();

#pragma warning disable IL2026
        ActionBase action = actionType switch
        {
            // File actions
            "CopyFile" => DeserializeAction<CopyFileAction>(rawText, options),
            "CreateFile" => DeserializeAction<CreateFileAction>(rawText, options),
            "DeleteFile" => DeserializeAction<DeleteFileAction>(rawText, options),
            "ExportToCSV" => DeserializeAction<ExportToCSVAction>(rawText, options),
            "ExportToPDF" => DeserializeAction<ExportToPDFAction>(rawText, options),
            "ImportCSV" => DeserializeAction<ImportCSVAction>(rawText, options),
            "MoveFile" => DeserializeAction<MoveFileAction>(rawText, options),
            "OpenFile" => DeserializeAction<OpenFileAction>(rawText, options),
            "ProtectFile" => DeserializeAction<ProtectFileAction>(rawText, options),
            "RenameFile" => DeserializeAction<RenameFileAction>(rawText, options),
            "SplitSheetsToFiles" => DeserializeAction<SplitSheetsToFilesAction>(rawText, options),
            "UnprotectFile" => DeserializeAction<UnprotectFileAction>(rawText, options),

            // Worksheet actions
            "AddNamedRange" => DeserializeAction<AddNamedRangeAction>(rawText, options),
            "AddSheet" => DeserializeAction<AddSheetAction>(rawText, options),
            "CopySheet" => DeserializeAction<CopySheetAction>(rawText, options),
            "DeleteHiddenSheets" => DeserializeAction<DeleteHiddenSheetsAction>(rawText, options),
            "DeleteSheet" => DeserializeAction<DeleteSheetAction>(rawText, options),
            "FreezePane" => DeserializeAction<FreezePaneAction>(rawText, options),
            "HideSheet" => DeserializeAction<HideSheetAction>(rawText, options),
            "MoveSheet" => DeserializeAction<MoveSheetAction>(rawText, options),
            "ProtectSheet" => DeserializeAction<ProtectSheetAction>(rawText, options),
            "RenameSheet" => DeserializeAction<RenameSheetAction>(rawText, options),
            "ReplaceSheet" => DeserializeAction<ReplaceSheetAction>(rawText, options),
            "SetHeaderFooter" => DeserializeAction<SetHeaderFooterAction>(rawText, options),
            "SetPageMargins" => DeserializeAction<SetPageMarginsAction>(rawText, options),
            "SetPageOrientation" => DeserializeAction<SetPageOrientationAction>(rawText, options),
            "SetPrintArea" => DeserializeAction<SetPrintAreaAction>(rawText, options),
            "SetSheetTabColor" => DeserializeAction<SetSheetTabColorAction>(rawText, options),
            "SetZoom" => DeserializeAction<SetZoomAction>(rawText, options),
            "RecalculateFormulas" => DeserializeAction<RecalculateFormulasAction>(rawText, options),
            "UnhideSheet" => DeserializeAction<UnhideSheetAction>(rawText, options),
            "UnprotectSheet" => DeserializeAction<UnprotectSheetAction>(rawText, options),

            // Data actions
            "CopyColumn" => DeserializeAction<CopyColumnAction>(rawText, options),
            "CopyRow" => DeserializeAction<CopyRowAction>(rawText, options),
            "DeduplicateRows" => DeserializeAction<DeduplicateRowsAction>(rawText, options),
            "DeleteColumn" => DeserializeAction<DeleteColumnAction>(rawText, options),
            "DeleteRow" => DeserializeAction<DeleteRowAction>(rawText, options),
            "FilterData" => DeserializeAction<FilterDataAction>(rawText, options),
            "GroupColumns" => DeserializeAction<GroupColumnsAction>(rawText, options),
            "GroupRows" => DeserializeAction<GroupRowsAction>(rawText, options),
            "InsertColumn" => DeserializeAction<InsertColumnAction>(rawText, options),
            "InsertRow" => DeserializeAction<InsertRowAction>(rawText, options),
            "MergeData" => DeserializeAction<MergeDataAction>(rawText, options),
            "MoveColumn" => DeserializeAction<MoveColumnAction>(rawText, options),
            "MoveRow" => DeserializeAction<MoveRowAction>(rawText, options),
            "SortData" => DeserializeAction<SortDataAction>(rawText, options),
            "UngroupColumns" => DeserializeAction<UngroupColumnsAction>(rawText, options),
            "UngroupRows" => DeserializeAction<UngroupRowsAction>(rawText, options),

            // Time actions
            "Wait" => DeserializeAction<WaitAction>(rawText, options),

            // Formatting actions
            "AutoFitColumns" => DeserializeAction<AutoFitColumnsAction>(rawText, options),
            "ConditionalFormatting" => DeserializeAction<ConditionalFormattingAction>(rawText, options),
            "FormatCells" => DeserializeAction<FormatCellsAction>(rawText, options),
            "SetColumnWidth" => DeserializeAction<SetColumnWidthAction>(rawText, options),
            "SetRowHeight" => DeserializeAction<SetRowHeightAction>(rawText, options),

            // Cell actions
            "AddHyperlink" => DeserializeAction<AddHyperlinkAction>(rawText, options),
            "ApplyFormula" => DeserializeAction<ApplyFormulaAction>(rawText, options),
            "ClearCells" => DeserializeAction<ClearCellsAction>(rawText, options),
            "CopyRange" => DeserializeAction<CopyRangeAction>(rawText, options),
            "FindAndReplace" => DeserializeAction<FindAndReplaceAction>(rawText, options),
            "MergeCells" => DeserializeAction<MergeCellsAction>(rawText, options),
            "SetCellValue" => DeserializeAction<SetCellValueAction>(rawText, options),
            "UnmergeCells" => DeserializeAction<UnmergeCellsAction>(rawText, options),
            "ValidateData" => DeserializeAction<ValidateDataAction>(rawText, options),

            // Advanced actions
            "CreatePivotTable" => DeserializeAction<CreatePivotTableAction>(rawText, options),
            "TransposeAction" => DeserializeAction<TransposeAction>(rawText, options),

            _ => throw new NotSupportedException($"Unknown action type: {actionType}")
        };
#pragma warning restore IL2026

        return action;
    }

    public override void Write(Utf8JsonWriter writer, ActionBase value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Create options without this converter to avoid infinite recursion
        var optionsForWriting = CreateOptionsWithoutThisConverter(options);
#pragma warning disable IL2026
        JsonSerializer.Serialize(writer, value, value.GetType(), optionsForWriting);
#pragma warning restore IL2026
    }

    [RequiresUnreferencedCode("Uses reflection-based JSON deserialization.")]
    private static T DeserializeAction<T>(string json, JsonSerializerOptions options) where T : ActionBase
    {
        try
        {
            // Create options without this converter to avoid infinite recursion
            var optionsForDeserialization = CreateOptionsWithoutThisConverter(options);
            var result = JsonSerializer.Deserialize<T>(json, optionsForDeserialization);

            return result ?? throw new JsonException($"Failed to deserialize action of type {typeof(T).Name}: result was null.");
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error deserializing action of type {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    private static JsonSerializerOptions CreateOptionsWithoutThisConverter(JsonSerializerOptions originalOptions)
    {
        var newOptions = new JsonSerializerOptions(originalOptions);

        // Remove this converter to avoid infinite recursion
        var converterToRemove = newOptions.Converters.FirstOrDefault(c => c is ActionJsonConverter);
        if (converterToRemove != null)
            newOptions.Converters.Remove(converterToRemove);

        return newOptions;
    }
}