using XLSXPipeline.Actions;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Actions.Advanced;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Models
{
    public class ActionJsonConverter : JsonConverter<ActionBase>
    {
        public override ActionBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var type = jsonDoc.RootElement.GetProperty("type").GetString();

            return type switch
            {
                // File Actions
                "CopyFile" => JsonSerializer.Deserialize<CopyFileAction>(jsonDoc.RootElement.GetRawText(), options),
                "ConvertToCSV" => JsonSerializer.Deserialize<ConvertToCSVAction>(jsonDoc.RootElement.GetRawText(), options),

                // Worksheet Actions
                "RenameSheet" => JsonSerializer.Deserialize<RenameSheetAction>(jsonDoc.RootElement.GetRawText(), options),
                "ProtectSheet" => JsonSerializer.Deserialize<ProtectSheetAction>(jsonDoc.RootElement.GetRawText(), options),

                // Data Actions
                "MoveColumn" => JsonSerializer.Deserialize<MoveColumnAction>(jsonDoc.RootElement.GetRawText(), options),
                "DeleteRow" => JsonSerializer.Deserialize<DeleteRowAction>(jsonDoc.RootElement.GetRawText(), options),
                "DeleteColumn" => JsonSerializer.Deserialize<DeleteColumnAction>(jsonDoc.RootElement.GetRawText(), options),
                "InsertRow" => JsonSerializer.Deserialize<InsertRowAction>(jsonDoc.RootElement.GetRawText(), options),
                "InsertColumn" => JsonSerializer.Deserialize<InsertColumnAction>(jsonDoc.RootElement.GetRawText(), options),
                "SortData" => JsonSerializer.Deserialize<SortDataAction>(jsonDoc.RootElement.GetRawText(), options),
                "FilterData" => JsonSerializer.Deserialize<FilterDataAction>(jsonDoc.RootElement.GetRawText(), options),
                "MergeData" => JsonSerializer.Deserialize<MergeDataAction>(jsonDoc.RootElement.GetRawText(), options),

                // Formatting Actions
                "FormatCells" => JsonSerializer.Deserialize<FormatCellsAction>(jsonDoc.RootElement.GetRawText(), options),

                // Cell Actions
                "SetCellValue" => JsonSerializer.Deserialize<SetCellValueAction>(jsonDoc.RootElement.GetRawText(), options),
                "ApplyFormula" => JsonSerializer.Deserialize<ApplyFormulaAction>(jsonDoc.RootElement.GetRawText(), options),
                "ValidateData" => JsonSerializer.Deserialize<ValidateDataAction>(jsonDoc.RootElement.GetRawText(), options),

                // Advanced Actions
                "CreatePivotTable" => JsonSerializer.Deserialize<CreatePivotTableAction>(jsonDoc.RootElement.GetRawText(), options),

                _ => throw new NotSupportedException($"Unknown action type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, ActionBase value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
