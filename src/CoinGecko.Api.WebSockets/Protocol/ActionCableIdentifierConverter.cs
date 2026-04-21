using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Protocol;

/// <summary>Reads / writes <see cref="ActionCableIdentifier"/> as a JSON-encoded string (ActionCable's double-encoded wire format).</summary>
public sealed class ActionCableIdentifierConverter : JsonConverter<ActionCableIdentifier>
{
    /// <inheritdoc/>
    public override ActionCableIdentifier? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token for ActionCable identifier, got {reader.TokenType}.");
        }

        var json = reader.GetString();
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        // Inner JSON is a small, fixed-shape object. Parse it manually to avoid re-entering this
        // converter (which would cause infinite recursion since [JsonConverter] is on the type).
        var doc = JsonDocument.Parse(json);
        var channel = doc.RootElement.GetProperty("channel").GetString() ?? string.Empty;
        return new ActionCableIdentifier { Channel = channel };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ActionCableIdentifier value, JsonSerializerOptions options)
    {
        // Manually produce {"channel":"<name>"} as a JSON-encoded string to avoid re-entering
        // this converter (which would cause infinite recursion since [JsonConverter] is on the type).
        using var ms = new MemoryStream();
        using (var inner = new Utf8JsonWriter(ms))
        {
            inner.WriteStartObject();
            inner.WriteString("channel", value.Channel);
            inner.WriteEndObject();
        }

        writer.WriteStringValue(Encoding.UTF8.GetString(ms.ToArray()));
    }
}
