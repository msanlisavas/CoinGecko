using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoinGecko.Api.Models;

namespace CoinGecko.Api.Serialization;

/// <summary>Reads a CoinGecko supply-chart two-element array <c>[ms, "supply_value"]</c> into <see cref="SupplyPoint"/>.</summary>
public sealed class SupplyPointConverter : JsonConverter<SupplyPoint>
{
    /// <inheritdoc/>
    public override SupplyPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected StartArray, got {reader.TokenType}.");
        }

        reader.Read();
        var ms = reader.GetInt64();

        reader.Read();
        decimal supply;
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString()!;
            supply = decimal.Parse(s, CultureInfo.InvariantCulture);
        }
        else
        {
            supply = reader.GetDecimal();
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected 2-element array.");
        }

        return new SupplyPoint(DateTimeOffset.FromUnixTimeMilliseconds(ms), supply);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SupplyPoint value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Timestamp.ToUnixTimeMilliseconds());
        writer.WriteStringValue(value.Supply.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndArray();
    }
}
