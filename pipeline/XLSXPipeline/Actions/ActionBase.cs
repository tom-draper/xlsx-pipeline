using XLSXPipeline.Models;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions
{
    [JsonConverter(typeof(ActionJsonConverter))]
    public abstract class ActionBase
    {
        public required string Type { get; set; }
        public abstract Task ExecuteAsync(string filePath);
    }
}

