namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact search result hit for LLM consumption.</summary>
/// <param name="Kind">Entity kind: <c>"coin"</c>, <c>"nft"</c>, <c>"exchange"</c>, or <c>"category"</c>.</param>
/// <param name="Id">CoinGecko id for the matched entity.</param>
/// <param name="Symbol">Ticker symbol (empty for exchanges and categories).</param>
/// <param name="Name">Display name of the matched entity.</param>
/// <param name="Rank">Market-cap rank, if available (coins only).</param>
public sealed record CoinSearchHit(
    string Kind,
    string Id,
    string Symbol,
    string Name,
    int? Rank);
