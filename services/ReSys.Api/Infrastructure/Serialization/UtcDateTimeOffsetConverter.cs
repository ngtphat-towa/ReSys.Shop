using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReSys.Api.Infrastructure.Serialization;

/// <summary>
/// Forces DateTimeOffset serialization to end in 'Z' (UTC) instead of '+00:00'.
/// </summary>
public class UtcDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && DateTimeOffset.TryParse(reader.GetString(), out var result))
        {
            return result;
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        // Force conversion to UTC and format with 'Z'
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"));
    }
}
