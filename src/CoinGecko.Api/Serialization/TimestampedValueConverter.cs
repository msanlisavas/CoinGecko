using System.Text.Json;
using System.Text.Json.Serialization;
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Serialization;

/// <summary>Reads a CoinGecko two-element array <c>[ms, value]</c> into <see cref="TimestampedValue"/>.</summary>
public sealed class TimestampedValueConverter : JsonConverter<TimestampedValue>
{
    /// <inheritdoc/>
    public override TimestampedValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected StartArray, got {reader.TokenType}.");
        }

        reader.Read();
        var ms = reader.GetInt64();

        reader.Read();
        var value = reader.GetDecimal();

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected 2-element array.");
        }

        return new TimestampedValue(DateTimeOffset.FromUnixTimeMilliseconds(ms), value);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TimestampedValue value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Timestamp.ToUnixTimeMilliseconds());
        writer.WriteNumberValue(value.Value);
        writer.WriteEndArray();
    }
}
