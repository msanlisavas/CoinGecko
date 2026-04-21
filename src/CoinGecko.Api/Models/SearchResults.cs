using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Response from <c>GET /search</c>.</summary>
public sealed class SearchResults
{
    /// <summary>Matching coins.</summary>
    [JsonPropertyName("coins")] public IReadOnlyList<SearchCoinHit> Coins { get; init; } = Array.Empty<SearchCoinHit>();

    /// <summary>Matching exchanges.</summary>
    [JsonPropertyName("exchanges")] public IReadOnlyList<SearchExchangeHit> Exchanges { get; init; } = Array.Empty<SearchExchangeHit>();

    /// <summary>Matching categories.</summary>
    [JsonPropertyName("categories")] public IReadOnlyList<SearchCategoryHit> Categories { get; init; } = Array.Empty<SearchCategoryHit>();

    /// <summary>Matching NFT collections.</summary>
    [JsonPropertyName("nfts")] public IReadOnlyList<SearchNftHit> Nfts { get; init; } = Array.Empty<SearchNftHit>();
}

/// <summary>Coin hit in search results.</summary>
public sealed class SearchCoinHit
{
    /// <summary>CoinGecko id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>API id (same as id in most cases).</summary>
    [JsonPropertyName("api_symbol")] public string? ApiSymbol { get; init; }

    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }

    /// <summary>Market-cap rank.</summary>
    [JsonPropertyName("market_cap_rank")] public int? MarketCapRank { get; init; }

    /// <summary>Small icon URL.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; init; }

    /// <summary>Large icon URL.</summary>
    [JsonPropertyName("large")] public string? Large { get; init; }
}

/// <summary>Exchange hit in search results.</summary>
public sealed class SearchExchangeHit
{
    /// <summary>Exchange id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Market type (e.g. <c>"Spot"</c>).</summary>
    [JsonPropertyName("market_type")] public string? MarketType { get; init; }

    /// <summary>Small icon URL.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; init; }

    /// <summary>Large icon URL.</summary>
    [JsonPropertyName("large")] public string? Large { get; init; }
}

/// <summary>Category hit in search results.</summary>
public sealed class SearchCategoryHit
{
    /// <summary>Category id. CoinGecko returns a slug string here (e.g. <c>"artificial-intelligence"</c>), not a numeric id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
}

/// <summary>NFT hit in search results.</summary>
public sealed class SearchNftHit
{
    /// <summary>NFT id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }

    /// <summary>Small icon URL.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; init; }
}
