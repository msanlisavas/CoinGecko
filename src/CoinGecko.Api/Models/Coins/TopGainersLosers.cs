using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Top gainers and losers from <c>/coins/top_gainers_losers</c>.</summary>
public sealed class TopGainersLosers
{
    /// <summary>Coins with highest gains in the requested window.</summary>
    [JsonPropertyName("top_gainers")] public IReadOnlyList<CoinMarket> TopGainers { get; init; } = Array.Empty<CoinMarket>();
    /// <summary>Coins with largest losses in the requested window.</summary>
    [JsonPropertyName("top_losers")] public IReadOnlyList<CoinMarket> TopLosers { get; init; } = Array.Empty<CoinMarket>();
}
