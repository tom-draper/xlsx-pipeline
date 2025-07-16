using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExcelPipeline.Models
{
    public class ActionJsonConverter : JsonConverter<ActionBase>
    {
        public override ActionBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var type = jsonDoc.RootElement.GetProperty("type").GetString();

            return type switch
            {
                "RenameSheet" => JsonSerializer.Deserialize<RenameSheetAction>(jsonDoc.RootElement.GetRawText(), options),
                "MoveColumn" => JsonSerializer.Deserialize<MoveColumnAction>(jsonDoc.RootElement.GetRawText(), options),
                "CopyFile" => JsonSerializer.Deserialize<CopyFileAction>(jsonDoc.RootElement.GetRawText(), options),
                _ => throw new NotSupportedException($"Unknown action type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, ActionBase value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
