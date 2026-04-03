using System.Text.Json;
using System.Text.Json.Serialization;

namespace XLSXPipeline.Actions;

/// <summary>
/// A string that supports date/time placeholder substitution (e.g. {year}, {month}, {day})
/// at the point of use rather than at deserialization time. The raw template is preserved
/// for serialization so placeholders remain intact in the stored JSON.
/// </summary>
[JsonConverter(typeof(PlaceholderStringConverter))]
public sealed class PlaceholderString
{
    public PlaceholderString(string raw) => Raw = raw;

    /// <summary>The original template string as stored in JSON.</summary>
    public string Raw { get; }

    /// <summary>The template with all date/time placeholders resolved to current values.</summary>
    public string Resolved => Helpers.ReplaceDateTimePlaceholders(Raw);

    public static implicit operator PlaceholderString?(string? s) => s is null ? null : new PlaceholderString(s);
    public static implicit operator string?(PlaceholderString? s) => s?.Resolved;

    public override string ToString() => Resolved;
}

public sealed class PlaceholderStringConverter : JsonConverter<PlaceholderString>
{
    public override PlaceholderString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var raw = reader.GetString();
        return raw is null ? null : new PlaceholderString(raw);
    }

    public override void Write(Utf8JsonWriter writer, PlaceholderString value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Raw);
}
