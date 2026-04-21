using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Serialization;

/// <summary>
/// Tolerant <see cref="DateTimeOffset"/> converter for CoinGecko responses. CoinGecko returns timestamps in
/// several shapes across endpoints: integer Unix seconds, fractional Unix seconds, numeric strings, and
/// ISO-8601 strings (e.g. <c>"2021-11-10T14:24:11.849Z"</c>). This converter handles all of them on read;
/// it writes as ISO-8601 round-trip format to preserve information.
/// </summary>
public sealed class UnixSecondsConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc/>
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var seconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            }

            var d = reader.GetDouble();
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(d * 1000));
        }

        if (reader.TokenType is JsonTokenType.String)
        {
            var s = reader.GetString();

            // Try numeric-string first (some endpoints stringify unix seconds).
            if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var secs))
            {
                return DateTimeOffset.FromUnixTimeSeconds(secs);
            }

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds((long)(d * 1000));
            }

            // Fall back to ISO-8601 / RFC 3339 parsing (the common case for fields like
            // ath_date, atl_date, last_updated, last_traded_at).
            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var iso))
            {
                return iso;
            }
        }

        throw new JsonException($"Cannot convert token {reader.TokenType} ({(reader.TokenType == JsonTokenType.String ? reader.GetString() : "")}) to DateTimeOffset.");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
    }
}
