namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact crypto-news article for LLM consumption.</summary>
/// <param name="Title">Article headline.</param>
/// <param name="Url">Canonical URL.</param>
/// <param name="Author">Author byline, if available.</param>
/// <param name="PostedAt">ISO-8601 publish timestamp.</param>
/// <param name="SourceName">Publisher name.</param>
/// <param name="Type"><c>"news"</c> or <c>"guide"</c>.</param>
/// <param name="RelatedCoinIds">CoinGecko coin ids referenced by the article.</param>
public sealed record NewsSummary(
    string Title,
    string Url,
    string? Author,
    string? PostedAt,
    string? SourceName,
    string Type,
    IReadOnlyList<string>? RelatedCoinIds);
