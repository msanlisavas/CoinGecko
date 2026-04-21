using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Full detail for a single coin from <c>/coins/{id}</c>.</summary>
public sealed class Coin
{
    /// <summary>CoinGecko id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Web slug (often same as id).</summary>
    [JsonPropertyName("web_slug")] public string? WebSlug { get; init; }
    /// <summary>Asset platform id (if the coin is a token).</summary>
    [JsonPropertyName("asset_platform_id")] public string? AssetPlatformId { get; init; }
    /// <summary>Platforms → contract address.</summary>
    [JsonPropertyName("platforms")] public IReadOnlyDictionary<string, string?>? Platforms { get; init; }
    /// <summary>Detailed platform contract info keyed by platform id.</summary>
    [JsonPropertyName("detail_platforms")] public IReadOnlyDictionary<string, CoinPlatformDetail>? DetailPlatforms { get; init; }
    /// <summary>Block time in minutes.</summary>
    [JsonPropertyName("block_time_in_minutes")] public decimal? BlockTimeInMinutes { get; init; }
    /// <summary>Hashing algorithm.</summary>
    [JsonPropertyName("hashing_algorithm")] public string? HashingAlgorithm { get; init; }
    /// <summary>Categories.</summary>
    [JsonPropertyName("categories")] public IReadOnlyList<string>? Categories { get; init; }
    /// <summary>Preview listing flag.</summary>
    [JsonPropertyName("preview_listing")] public bool? PreviewListing { get; init; }
    /// <summary>Public notice.</summary>
    [JsonPropertyName("public_notice")] public string? PublicNotice { get; init; }
    /// <summary>Additional notices.</summary>
    [JsonPropertyName("additional_notices")] public IReadOnlyList<string>? AdditionalNotices { get; init; }
    /// <summary>Localization dictionary.</summary>
    [JsonPropertyName("localization")] public IReadOnlyDictionary<string, string?>? Localization { get; init; }
    /// <summary>Multi-language descriptions.</summary>
    [JsonPropertyName("description")] public IReadOnlyDictionary<string, string?>? Description { get; init; }
    /// <summary>Links.</summary>
    [JsonPropertyName("links")] public CoinLinks? Links { get; init; }
    /// <summary>Image variants.</summary>
    [JsonPropertyName("image")] public CoinImage? Image { get; init; }
    /// <summary>Country of origin.</summary>
    [JsonPropertyName("country_origin")] public string? CountryOrigin { get; init; }
    /// <summary>Genesis date (ISO-8601).</summary>
    [JsonPropertyName("genesis_date")] public string? GenesisDate { get; init; }
    /// <summary>Community sentiment up percentage.</summary>
    [JsonPropertyName("sentiment_votes_up_percentage")] public decimal? SentimentVotesUpPercentage { get; init; }
    /// <summary>Community sentiment down percentage.</summary>
    [JsonPropertyName("sentiment_votes_down_percentage")] public decimal? SentimentVotesDownPercentage { get; init; }
    /// <summary>Watchlist portfolio users.</summary>
    [JsonPropertyName("watchlist_portfolio_users")] public long? WatchlistPortfolioUsers { get; init; }
    /// <summary>Market-cap rank.</summary>
    [JsonPropertyName("market_cap_rank")] public int? MarketCapRank { get; init; }
    /// <summary>Full market data.</summary>
    [JsonPropertyName("market_data")] public CoinMarketData? MarketData { get; init; }
    /// <summary>Community data.</summary>
    [JsonPropertyName("community_data")] public CoinCommunityData? CommunityData { get; init; }
    /// <summary>Developer data.</summary>
    [JsonPropertyName("developer_data")] public CoinDeveloperData? DeveloperData { get; init; }
    /// <summary>Status updates feed.</summary>
    [JsonPropertyName("status_updates")] public IReadOnlyList<StatusUpdate>? StatusUpdates { get; init; }
    /// <summary>Last updated timestamp.</summary>
    [JsonPropertyName("last_updated")] public DateTimeOffset? LastUpdated { get; init; }
    /// <summary>Embedded top tickers.</summary>
    [JsonPropertyName("tickers")] public IReadOnlyList<Ticker>? Tickers { get; init; }
}

/// <summary>Detailed per-platform contract info.</summary>
public sealed class CoinPlatformDetail
{
    /// <summary>Decimal precision.</summary>
    [JsonPropertyName("decimal_place")] public int? DecimalPlace { get; init; }
    /// <summary>Contract address.</summary>
    [JsonPropertyName("contract_address")] public string? ContractAddress { get; init; }
}
