using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class DerivativesClient(HttpClient http) : IDerivativesClient
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<Derivative>> GetTickersAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "derivatives");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.DerivativeArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /derivatives.");
    }

    public async Task<IReadOnlyList<DerivativeExchange>> GetExchangesAsync(
        DerivativeExchangesOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("order", options?.Order)
            .Add("per_page", options?.PerPage)
            .Add("page", options?.Page);
        using var req = new HttpRequestMessage(HttpMethod.Get, "derivatives/exchanges" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.DerivativeExchangeArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /derivatives/exchanges.");
    }

    public async Task<DerivativeExchangeDetail> GetExchangeAsync(
        string id, bool includeTickers = false, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("derivatives/exchanges/{id}", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .Add("include_tickers", includeTickers ? "all" : null);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.DerivativeExchangeDetail, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /derivatives/exchanges/{id}.");
    }

    public async Task<IReadOnlyList<DerivativeExchangeListItem>> GetExchangeListAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "derivatives/exchanges/list");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.DerivativeExchangeListItemArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /derivatives/exchanges/list.");
    }
}
