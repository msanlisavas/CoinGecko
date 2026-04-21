namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Aggregated trending data across coins, NFTs, and categories for LLM consumption.</summary>
/// <param name="Coins">Currently trending coins.</param>
/// <param name="Nfts">Currently trending NFT collections.</param>
/// <param name="Categories">Currently trending coin categories.</param>
public sealed record TrendingSummary(
    IReadOnlyList<TrendingCoinSummary> Coins,
    IReadOnlyList<TrendingNftSummary> Nfts,
    IReadOnlyList<TrendingCategorySummary> Categories);

/// <summary>Compact trending coin entry for LLM consumption.</summary>
/// <param name="CoinId">CoinGecko id of the trending coin.</param>
/// <param name="Symbol">Ticker symbol.</param>
/// <param name="Name">Display name.</param>
/// <param name="MarketCapRank">Market-cap rank, if available.</param>
public sealed record TrendingCoinSummary(
    string CoinId,
    string Symbol,
    string Name,
    int? MarketCapRank);

/// <summary>Compact trending NFT collection entry for LLM consumption.</summary>
/// <param name="Id">CoinGecko NFT collection id.</param>
/// <param name="Name">Display name of the collection.</param>
/// <param name="Symbol">Ticker symbol of the collection.</param>
/// <param name="FloorPrice24hChangePercent">24h floor price percentage change.</param>
public sealed record TrendingNftSummary(
    string Id,
    string Name,
    string Symbol,
    decimal? FloorPrice24hChangePercent);

/// <summary>Compact trending category entry for LLM consumption.</summary>
/// <param name="Id">CoinGecko category id.</param>
/// <param name="Name">Display name of the category.</param>
/// <param name="MarketCap1hChange">1-hour market-cap percentage change for the category.</param>
public sealed record TrendingCategorySummary(
    string Id,
    string Name,
    decimal? MarketCap1hChange);
