using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>A newly listed coin from <c>/coins/list/new</c>.</summary>
public sealed class NewCoinListing
{
    /// <summary>CoinGecko id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Unix timestamp when the coin was activated.</summary>
    [JsonPropertyName("activated_at")] public long? ActivatedAt { get; init; }
}
