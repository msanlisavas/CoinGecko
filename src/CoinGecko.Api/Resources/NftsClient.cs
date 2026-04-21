using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class NftsClient(HttpClient http) : INftsClient
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<NftListItem>> GetListAsync(
        int perPage = 100, int page = 1, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("per_page", perPage)
            .Add("page", page);
        using var req = new HttpRequestMessage(HttpMethod.Get, "nfts/list" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.NftListItemArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /nfts/list.");
    }

    public async Task<Nft> GetAsync(string id, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("nfts/{id}", new[] { ("id", id) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.Nft, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /nfts/{id}.");
    }

    public async Task<Nft> GetByContractAsync(
        string assetPlatformId, string contractAddress, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand(
            "nfts/{asset_platform_id}/contract/{contract_address}",
            new[] { ("asset_platform_id", assetPlatformId), ("contract_address", contractAddress) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.Nft, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /nfts/{assetPlatformId}/contract/{contractAddress}.");
    }

    public async Task<IReadOnlyList<NftMarket>> GetMarketsAsync(
        NftMarketsOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("asset_platform_id", options?.AssetPlatformId)
            .Add("order", options?.Order)
            .Add("per_page", options?.PerPage)
            .Add("page", options?.Page);
        using var req = new HttpRequestMessage(HttpMethod.Get, "nfts/markets" + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.NftMarketArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /nfts/markets.");
    }

    public async Task<IReadOnlyList<NftMarketChartPoint>> GetMarketChartAsync(
        string id, int days, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("nfts/{id}/market_chart", new[] { ("id", id) });
        var qs = new QueryStringBuilder().Add("days", days);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.NftMarketChartRaw, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /nfts/{id}/market_chart.");
        return MergeMarketChart(raw);
    }

    public async Task<NftTickers> GetTickersAsync(string id, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("nfts/{id}/tickers", new[] { ("id", id) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.NftTickers, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /nfts/{id}/tickers.");
    }

    private static NftMarketChartPoint[] MergeMarketChart(NftMarketChartRaw raw)
    {
        var native = raw.FloorPriceNative;
        var usd    = raw.FloorPriceUsd;
        var cap    = raw.MarketCapNative;
        var vol    = raw.H24VolumeNative;

        var len = native?.Length ?? usd?.Length ?? cap?.Length ?? vol?.Length ?? 0;
        var result = new NftMarketChartPoint[len];
        for (var i = 0; i < len; i++)
        {
            var ms = (long)(native?[i]?[0] ?? usd?[i]?[0] ?? cap?[i]?[0] ?? vol?[i]?[0] ?? 0m);
            result[i] = new NftMarketChartPoint
            {
                Timestamp        = DateTimeOffset.FromUnixTimeMilliseconds(ms),
                FloorPriceNative = native?[i]?[1],
                FloorPriceUsd    = usd?[i]?[1],
                MarketCapNative  = cap?[i]?[1],
                Volume24hNative  = vol?[i]?[1],
            };
        }
        return result;
    }
}

internal sealed class NftMarketChartRaw
{
    [JsonPropertyName("floor_price_usd")]    public decimal?[][]? FloorPriceUsd { get; init; }
    [JsonPropertyName("floor_price_native")] public decimal?[][]? FloorPriceNative { get; init; }
    [JsonPropertyName("market_cap_native")]  public decimal?[][]? MarketCapNative { get; init; }
    [JsonPropertyName("h24_volume_native")]  public decimal?[][]? H24VolumeNative { get; init; }
}
