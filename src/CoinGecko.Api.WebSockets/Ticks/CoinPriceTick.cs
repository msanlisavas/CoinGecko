using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>C1 channel tick — CoinGecko.com aggregated coin price.</summary>
public sealed class CoinPriceTick
{
    /// <summary>Channel code (always <c>"C1"</c>).</summary>
    [JsonPropertyName("c")] public string? ChannelCode { get; init; }
    /// <summary>Coin id.</summary>
    [JsonPropertyName("i")] public string? CoinId { get; init; }
    /// <summary>Quote currency code.</summary>
    [JsonPropertyName("vs")] public string? VsCurrency { get; init; }
    /// <summary>Current price in quote currency.</summary>
    [JsonPropertyName("p")] public decimal Price { get; init; }
    /// <summary>24-hour percentage change.</summary>
    [JsonPropertyName("pp")] public decimal? PricePercentChange24h { get; init; }
    /// <summary>Market cap.</summary>
    [JsonPropertyName("m")] public decimal? MarketCap { get; init; }
    /// <summary>24h volume.</summary>
    [JsonPropertyName("v")] public decimal? Volume24h { get; init; }
    /// <summary>UTC timestamp (parsed from unix seconds, possibly fractional).</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Raw <c>t</c> field (unix seconds as float, assigned by deserializer, translated to <see cref="Timestamp"/> on construction).</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal double RawT
    {
        get => Timestamp.ToUnixTimeMilliseconds() / 1000.0;
        init => Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)(value * 1000));
    }
}
