using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class TrendingClient(HttpClient http) : ITrendingClient
{
    private readonly HttpClient _http = http;

    public async Task<TrendingResults> GetAsync(IReadOnlyList<string>? showMax = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder().AddList("show_max", showMax);
        using var req = new HttpRequestMessage(HttpMethod.Get, "search/trending" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.TrendingResults, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /search/trending.");
    }
}
