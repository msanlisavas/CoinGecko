using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Serialization;

/// <summary>
/// Tolerant <see cref="decimal"/>? converter. CoinGecko occasionally emits non-numeric strings like
/// <c>"NaN"</c>, empty strings, or <c>null</c> literals in numeric fields (most visible in the
/// onchain / GeckoTerminal endpoints for freshly-created pools with no liquidity yet). This converter
/// treats such values as <c>null</c> instead of throwing a <see cref="JsonException"/>.
/// </summary>
/// <remarks>
/// For well-formed numbers (JSON numbers or numeric strings), parsing is identical to the STJ default.
/// Only pathological values collapse to <c>null</c>. The bundled global options already set
/// <see cref="JsonNumberHandling.AllowReadingFromString"/> so numeric strings continue to work.
/// </remarks>
public sealed class TolerantDecimalConverter : JsonConverter<decimal?>
{
    /// <inheritdoc/>
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.Number:
                if (reader.TryGetDecimal(out var d))
                {
                    return d;
                }
                // Doubles outside decimal range (NaN/∞): fall through to null.
                return null;

            case JsonTokenType.String:
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    return null;
                }
                if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
                // "NaN", "Infinity", "-Infinity", or any other non-decimal: null.
                return null;

            default:
                throw new JsonException($"Cannot convert token {reader.TokenType} to decimal?.");
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
