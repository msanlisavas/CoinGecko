using CoinGecko.Api.Models.News;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Crypto News endpoints. Available on Analyst, Lite, Pro, and Enterprise plans.</summary>
public interface INewsClient
{
    /// <summary>Returns crypto news (and optionally guides) aggregated from 100+ publishers. <c>GET /news</c></summary>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<NewsArticle[]> GetNewsAsync(NewsOptions? options = null, CancellationToken ct = default);
}
