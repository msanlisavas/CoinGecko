using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Historical snapshot for a coin from <c>/coins/{id}/history</c>.</summary>
public sealed class CoinHistory
{
    /// <summary>CoinGecko id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Localization dictionary.</summary>
    [JsonPropertyName("localization")] public IReadOnlyDictionary<string, string?>? Localization { get; init; }
    /// <summary>Image variants at that snapshot date.</summary>
    [JsonPropertyName("image")] public CoinImage? Image { get; init; }
    /// <summary>Market data at the snapshot date.</summary>
    [JsonPropertyName("market_data")] public CoinHistoryMarketData? MarketData { get; init; }
    /// <summary>Community data at the snapshot date.</summary>
    [JsonPropertyName("community_data")] public CoinCommunityData? CommunityData { get; init; }
    /// <summary>Developer data at the snapshot date.</summary>
    [JsonPropertyName("developer_data")] public CoinDeveloperData? DeveloperData { get; init; }
    /// <summary>Public interest stats at the snapshot date.</summary>
    [JsonPropertyName("public_interest_stats")] public CoinPublicInterestStats? PublicInterestStats { get; init; }
}

/// <summary>Simplified market data within a historical snapshot.</summary>
public sealed class CoinHistoryMarketData
{
    /// <summary>Current price at snapshot keyed by currency code.</summary>
    [JsonPropertyName("current_price")] public IReadOnlyDictionary<string, decimal?>? CurrentPrice { get; init; }
    /// <summary>Market cap at snapshot keyed by currency code.</summary>
    [JsonPropertyName("market_cap")] public IReadOnlyDictionary<string, decimal?>? MarketCap { get; init; }
    /// <summary>Total volume at snapshot keyed by currency code.</summary>
    [JsonPropertyName("total_volume")] public IReadOnlyDictionary<string, decimal?>? TotalVolume { get; init; }
}

/// <summary>Public interest statistics for a coin.</summary>
public sealed class CoinPublicInterestStats
{
    /// <summary>Alexa rank.</summary>
    [JsonPropertyName("alexa_rank")] public long? AlexaRank { get; init; }
    /// <summary>Bing matches.</summary>
    [JsonPropertyName("bing_matches")] public long? BingMatches { get; init; }
}
