using System.Text.Json;
using System.Text.Json.Serialization;
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Serialization;

/// <summary>Reads a CoinGecko five-element OHLC array <c>[ts, o, h, l, c]</c> into <see cref="CoinOhlc"/>.</summary>
public sealed class CoinOhlcConverter : JsonConverter<CoinOhlc>
{
    /// <inheritdoc/>
    public override CoinOhlc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        reader.Read();
        var ts = reader.GetInt64();
        reader.Read();
        var o = reader.GetDecimal();
        reader.Read();
        var h = reader.GetDecimal();
        reader.Read();
        var l = reader.GetDecimal();
        reader.Read();
        var c = reader.GetDecimal();
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException();
        }
        return new CoinOhlc(DateTimeOffset.FromUnixTimeMilliseconds(ts), o, h, l, c);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CoinOhlc value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Timestamp.ToUnixTimeMilliseconds());
        writer.WriteNumberValue(value.Open);
        writer.WriteNumberValue(value.High);
        writer.WriteNumberValue(value.Low);
        writer.WriteNumberValue(value.Close);
        writer.WriteEndArray();
    }
}
