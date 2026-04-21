namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact coin price snapshot for LLM consumption.</summary>
/// <param name="CoinId">CoinGecko id (e.g. <c>"bitcoin"</c>).</param>
/// <param name="Symbol">Ticker (<c>"BTC"</c>).</param>
/// <param name="Name">Display name.</param>
/// <param name="VsCurrency">Quote currency.</param>
/// <param name="Price">Current price.</param>
/// <param name="Change24hPercent">24-hour percentage change.</param>
/// <param name="MarketCap">Market capitalization in the quote currency.</param>
public sealed record CoinPriceQuote(
    string CoinId,
    string Symbol,
    string Name,
    string VsCurrency,
    decimal Price,
    decimal? Change24hPercent,
    decimal? MarketCap);
