namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact derivative ticker row for LLM consumption.</summary>
/// <param name="Market">Exchange or market name offering the derivative.</param>
/// <param name="Symbol">Trading pair symbol (e.g. <c>"BTC-USDT"</c>).</param>
/// <param name="Price">Last traded price as a string (CoinGecko returns derivatives prices as strings).</param>
/// <param name="Change24hPercent">24h price percentage change.</param>
/// <param name="FundingRate">Current funding rate for perpetuals, if applicable.</param>
/// <param name="Volume24hUsd">24h trading volume in USD.</param>
public sealed record DerivativeSummary(
    string Market,
    string Symbol,
    string? Price,
    decimal? Change24hPercent,
    decimal? FundingRate,
    decimal? Volume24hUsd);
