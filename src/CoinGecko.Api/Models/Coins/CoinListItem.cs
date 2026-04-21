using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>A row from <c>/coins/list</c> — id, symbol, name, and optional platform contract addresses.</summary>
public sealed class CoinListItem
{
    /// <summary>CoinGecko id (e.g. <c>"bitcoin"</c>).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>When <c>include_platform=true</c>, maps platform id → contract address.</summary>
    [JsonPropertyName("platforms")] public IReadOnlyDictionary<string, string?>? Platforms { get; init; }
}
