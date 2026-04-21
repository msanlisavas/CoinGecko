namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact market row for LLM consumption.</summary>
/// <param name="Rank">Market-cap rank.</param>
/// <param name="CoinId">CoinGecko id.</param>
/// <param name="Symbol">Ticker.</param>
/// <param name="Name">Display name.</param>
/// <param name="Price">Current price in USD.</param>
/// <param name="Change24hPercent">24h change percentage.</param>
/// <param name="MarketCap">Market cap in USD.</param>
/// <param name="Volume24h">24h trading volume in USD.</param>
public sealed record MarketSnapshot(
    int? Rank,
    string CoinId,
    string Symbol,
    string Name,
    decimal? Price,
    decimal? Change24hPercent,
    decimal? MarketCap,
    decimal? Volume24h);
