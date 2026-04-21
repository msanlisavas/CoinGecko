using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Market chart data from <c>/coins/{id}/market_chart</c> and related endpoints.</summary>
public sealed class MarketChart
{
    /// <summary>Price series — each item is a (timestamp, price) pair.</summary>
    [JsonPropertyName("prices")] public IReadOnlyList<TimestampedValue> Prices { get; init; } = Array.Empty<TimestampedValue>();
    /// <summary>Market-cap series — each item is a (timestamp, market_cap) pair.</summary>
    [JsonPropertyName("market_caps")] public IReadOnlyList<TimestampedValue> MarketCaps { get; init; } = Array.Empty<TimestampedValue>();
    /// <summary>Total-volume series — each item is a (timestamp, volume) pair.</summary>
    [JsonPropertyName("total_volumes")] public IReadOnlyList<TimestampedValue> TotalVolumes { get; init; } = Array.Empty<TimestampedValue>();
}
