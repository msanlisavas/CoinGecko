using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Coin tickers paged response from <c>/coins/{id}/tickers</c>.</summary>
public sealed class CoinTickers
{
    /// <summary>Coin display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Tickers on this page.</summary>
    [JsonPropertyName("tickers")] public IReadOnlyList<Ticker> Tickers { get; init; } = Array.Empty<Ticker>();
}
