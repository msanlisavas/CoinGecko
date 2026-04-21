using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Serialization;

/// <summary>Reads Unix timestamps as numeric seconds (integer or fractional) and writes integer seconds.</summary>
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
            if (long.TryParse(s, out var secs))
            {
                return DateTimeOffset.FromUnixTimeSeconds(secs);
            }

            if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds((long)(d * 1000));
            }
        }

        throw new JsonException($"Cannot convert token {reader.TokenType} to DateTimeOffset (Unix seconds).");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
