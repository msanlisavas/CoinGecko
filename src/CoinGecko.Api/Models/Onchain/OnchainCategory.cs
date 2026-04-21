using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>An onchain category (e.g. "ai", "defi").</summary>
public sealed class OnchainCategory
{
    /// <summary>Category id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Category attributes.</summary>
    [JsonPropertyName("attributes")] public OnchainCategoryAttributes? Attributes { get; init; }
}

/// <summary>Category display + aggregate data.</summary>
public sealed class OnchainCategoryAttributes
{
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Description.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    /// <summary>Market cap USD.</summary>
    [JsonPropertyName("market_cap_usd")] public decimal? MarketCapUsd { get; init; }
    /// <summary>24h volume USD.</summary>
    [JsonPropertyName("volume_usd")] public decimal? VolumeUsd { get; init; }
    /// <summary>24h change percentage.</summary>
    [JsonPropertyName("market_cap_change_percentage")] public IReadOnlyDictionary<string, decimal?>? MarketCapChangePercentage { get; init; }
    /// <summary>Number of pools.</summary>
    [JsonPropertyName("pools_count")] public int? PoolsCount { get; init; }
}
