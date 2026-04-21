using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>A category entry from <c>/coins/categories/list</c>.</summary>
public sealed class CategoryListItem
{
    /// <summary>Category id (slug).</summary>
    [JsonPropertyName("category_id")] public string? CategoryId { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
}

/// <summary>A category with market data from <c>/coins/categories</c>.</summary>
public sealed class CoinCategory
{
    /// <summary>Category id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Total market cap in USD.</summary>
    [JsonPropertyName("market_cap")] public decimal? MarketCap { get; init; }

    /// <summary>24h market-cap change percentage.</summary>
    [JsonPropertyName("market_cap_change_24h")] public decimal? MarketCapChange24h { get; init; }

    /// <summary>Category content snippet.</summary>
    [JsonPropertyName("content")] public string? Content { get; init; }

    /// <summary>Top 3 coin icons.</summary>
    [JsonPropertyName("top_3_coins")] public IReadOnlyList<string>? Top3Coins { get; init; }

    /// <summary>24h trading volume in USD.</summary>
    [JsonPropertyName("volume_24h")] public decimal? Volume24h { get; init; }

    /// <summary>Last updated timestamp (ISO-8601).</summary>
    [JsonPropertyName("updated_at")] public string? UpdatedAt { get; init; }
}
