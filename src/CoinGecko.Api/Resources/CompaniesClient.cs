using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class CompaniesClient(HttpClient http) : ICompaniesClient
{
    private readonly HttpClient _http = http;

    public async Task<CompanyTreasury> GetPublicTreasuryAsync(string coinId, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("companies/public_treasury/{coin_id}", new[] { ("coin_id", coinId) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CompanyTreasury, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /companies/public_treasury/{coin_id}.");
    }
}
