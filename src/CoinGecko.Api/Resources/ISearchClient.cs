using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Search endpoints.</summary>
public interface ISearchClient
{
    /// <summary>Calls <c>GET /search</c>. Returns coins, exchanges, categories, and NFTs matching the query.</summary>
    Task<SearchResults> SearchAsync(string query, CancellationToken ct = default);
}
