using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Categories endpoints.</summary>
public interface ICategoriesClient
{
    /// <summary>Calls <c>GET /coins/categories/list</c>. Returns a flat list of all category ids and names.</summary>
    Task<IReadOnlyList<CategoryListItem>> GetListAsync(CancellationToken ct = default);

    /// <summary>Calls <c>GET /coins/categories</c>. Returns categories with market data, optionally sorted by <paramref name="order"/>.</summary>
    Task<IReadOnlyList<CoinCategory>> GetAsync(string? order = null, CancellationToken ct = default);
}
