using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class SearchClient(HttpClient http) : ISearchClient
{
    private readonly HttpClient _http = http;

    public async Task<SearchResults> SearchAsync(string query, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder().Add("query", query);
        using var req = new HttpRequestMessage(HttpMethod.Get, "search" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.SearchResults, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /search.");
    }
}
