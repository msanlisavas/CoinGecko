using System.Net.Http.Json;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class KeyClient(HttpClient http) : IKeyClient
{
    private readonly HttpClient _http = http;

    public async Task<ApiKeyUsage> GetAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "key");
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.ApiKeyUsage, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /key.");
    }
}
