using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Trending endpoints.</summary>
public interface ITrendingClient
{
    /// <summary>Calls <c>GET /search/trending</c>. Returns trending coins, NFTs, and categories.</summary>
    Task<TrendingResults> GetAsync(IReadOnlyList<string>? showMax = null, CancellationToken ct = default);
}
