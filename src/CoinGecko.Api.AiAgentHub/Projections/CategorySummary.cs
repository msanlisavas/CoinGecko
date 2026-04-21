namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact coin category row with aggregate market data for LLM consumption.</summary>
/// <param name="Id">CoinGecko category id.</param>
/// <param name="Name">Display name of the category.</param>
/// <param name="MarketCapUsd">Aggregate market cap of all coins in the category, in USD.</param>
/// <param name="Volume24hUsd">Aggregate 24h trading volume for the category, in USD.</param>
/// <param name="Change24hPercent">24h market-cap percentage change for the category.</param>
public sealed record CategorySummary(
    string Id,
    string Name,
    decimal? MarketCapUsd,
    decimal? Volume24hUsd,
    decimal? Change24hPercent);
