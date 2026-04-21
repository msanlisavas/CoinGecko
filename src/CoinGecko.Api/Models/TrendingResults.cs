using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Response from <c>GET /search/trending</c>.</summary>
public sealed class TrendingResults
{
    /// <summary>Trending coins (up to 15 on Demo / 30 on Analyst+).</summary>
    [JsonPropertyName("coins")] public IReadOnlyList<TrendingCoinItem> Coins { get; init; } = Array.Empty<TrendingCoinItem>();

    /// <summary>Trending NFT collections.</summary>
    [JsonPropertyName("nfts")] public IReadOnlyList<TrendingNftItem> Nfts { get; init; } = Array.Empty<TrendingNftItem>();

    /// <summary>Trending categories.</summary>
    [JsonPropertyName("categories")] public IReadOnlyList<TrendingCategoryItem> Categories { get; init; } = Array.Empty<TrendingCategoryItem>();
}

/// <summary>A trending coin row.</summary>
public sealed class TrendingCoinItem
{
    /// <summary>Inner <c>item</c> object with the coin metadata.</summary>
    [JsonPropertyName("item")] public TrendingCoinData? Item { get; init; }
}

/// <summary>Coin metadata inside a trending row.</summary>
public sealed class TrendingCoinData
{
    /// <summary>Coin id (e.g. <c>"bitcoin"</c>).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>CoinGecko numeric rank id.</summary>
    [JsonPropertyName("coin_id")] public long? CoinId { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }

    /// <summary>Market-cap rank at the time of the snapshot.</summary>
    [JsonPropertyName("market_cap_rank")] public int? MarketCapRank { get; init; }

    /// <summary>Small icon URL.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; init; }

    /// <summary>Large icon URL.</summary>
    [JsonPropertyName("large")] public string? Large { get; init; }

    /// <summary>Relative trending score (0 = top).</summary>
    [JsonPropertyName("score")] public int? Score { get; init; }
}

/// <summary>A trending NFT collection row.</summary>
public sealed class TrendingNftItem
{
    /// <summary>NFT id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }

    /// <summary>Small icon URL.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; init; }

    /// <summary>24-hour floor price change in native currency.</summary>
    [JsonPropertyName("floor_price_24h_percentage_change")] public decimal? FloorPrice24hPercentageChange { get; init; }
}

/// <summary>A trending category row.</summary>
public sealed class TrendingCategoryItem
{
    /// <summary>Category id.</summary>
    [JsonPropertyName("id")] public long? Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Market-cap change percentage (24h, USD).</summary>
    [JsonPropertyName("market_cap_1h_change")] public decimal? MarketCap1hChange { get; init; }
}
