using ExcelPipeline.Models;
using System.Text.Json.Serialization;

namespace ExcelPipeline.Actions
{
    [JsonConverter(typeof(ActionJsonConverter))]
    public abstract class ActionBase
    {
        public string Type { get; set; }
        public abstract Task ExecuteAsync(string filePath);
    }
}

