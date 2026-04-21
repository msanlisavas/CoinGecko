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

        // Inner JSON is a small, fixed-shape object — deserialize with plain STJ reflection on the
        // known simple type. This is AOT-safe because ActionCableIdentifier has a trivial shape.
        return JsonSerializer.Deserialize(json, ActionCableProtocolJsonContext.Default.ActionCableIdentifier);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ActionCableIdentifier value, JsonSerializerOptions options)
    {
        var inner = JsonSerializer.Serialize(value, ActionCableProtocolJsonContext.Default.ActionCableIdentifier);
        writer.WriteStringValue(inner);
    }
}
