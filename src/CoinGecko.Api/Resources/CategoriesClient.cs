using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class CategoriesClient(HttpClient http) : ICategoriesClient
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<CategoryListItem>> GetListAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "coins/categories/list");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CategoryListItemArray, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /coins/categories/list.");
    }

    public async Task<IReadOnlyList<CoinCategory>> GetAsync(string? order = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder().Add("order", order);
        using var req = new HttpRequestMessage(HttpMethod.Get, "coins/categories" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CoinCategoryArray, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /coins/categories.");
    }
}
