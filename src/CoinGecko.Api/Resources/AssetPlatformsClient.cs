using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class AssetPlatformsClient(HttpClient http) : IAssetPlatformsClient
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<AssetPlatform>> GetListAsync(string? filter = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder().Add("filter", filter);
        using var req = new HttpRequestMessage(HttpMethod.Get, "asset_platforms" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.AssetPlatformArray, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /asset_platforms.");
    }

    public async Task<TokenList> GetTokenListAsync(string assetPlatformId, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("token_lists/{asset_platform_id}/all.json", new[] { ("asset_platform_id", assetPlatformId) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.TokenList, ct).ConfigureAwait(false);
        return dto ?? throw new InvalidOperationException("CoinGecko returned empty body for /token_lists/{asset_platform_id}/all.json.");
    }
}
