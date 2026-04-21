using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class ExchangesClient(HttpClient http) : IExchangesClient
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<Exchange>> GetAsync(
        ExchangesOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("per_page", options?.PerPage)
            .Add("page", options?.Page);
        using var req = new HttpRequestMessage(HttpMethod.Get, "exchanges" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.ExchangeArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /exchanges.");
    }

    public async Task<IReadOnlyList<ExchangeListItem>> GetListAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "exchanges/list");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.ExchangeListItemArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /exchanges/list.");
    }

    public async Task<ExchangeDetail> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("exchanges/{id}", new[] { ("id", id) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.ExchangeDetail, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /exchanges/{id}.");
    }

    public async Task<ExchangeTickers> GetTickersAsync(
        string id, ExchangeTickersOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("exchanges/{id}/tickers", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddList("coin_ids", options?.CoinIds)
            .Add("include_exchange_logo", options?.IncludeExchangeLogo == true ? "true" : null)
            .Add("page", options?.Page)
            .Add("depth", options?.Depth == true ? "true" : null)
            .Add("order", options?.Order)
            .Add("dex_pair_format", options?.DexPairFormat);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.ExchangeTickers, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /exchanges/{id}/tickers.");
    }

    public async Task<IReadOnlyList<ExchangeVolumeChartPoint>> GetVolumeChartAsync(
        string id, int days, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("exchanges/{id}/volume_chart", new[] { ("id", id) });
        var qs = new QueryStringBuilder().Add("days", days);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.StringArrayArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /exchanges/{id}/volume_chart.");
        return ProjectVolumeChart(raw);
    }

    public async Task<IReadOnlyList<ExchangeVolumeChartPoint>> GetVolumeChartRangeAsync(
        string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("exchanges/{id}/volume_chart/range", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddUnixSeconds("from", from)
            .AddUnixSeconds("to", to);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.StringArrayArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /exchanges/{id}/volume_chart/range.");
        return ProjectVolumeChart(raw);
    }

    public IAsyncEnumerable<Exchange> EnumerateAsync(
        ExchangesOptions? options = null, CancellationToken ct = default)
    {
        var baseOptions = options ?? new ExchangesOptions();
        return PaginationHelper.EnumerateAsync<Exchange>(
            fetchPage: async (page, c) =>
                await GetAsync(baseOptions with { Page = page }, c).ConfigureAwait(false),
            perPage: baseOptions.PerPage,
            ct: ct);
    }

    public IAsyncEnumerable<Ticker> EnumerateTickersAsync(
        string id, ExchangeTickersOptions? options = null, CancellationToken ct = default)
    {
        var baseOptions = options ?? new ExchangeTickersOptions();
        // Exchange tickers endpoint has no PerPage param; fixed 100/page.
        return PaginationHelper.EnumerateAsync<Ticker>(
            fetchPage: async (page, c) =>
            {
                var envelope = await GetTickersAsync(id, baseOptions with { Page = page }, c).ConfigureAwait(false);
                return envelope.Tickers;
            },
            perPage: 100,
            ct: ct);
    }

    private static ExchangeVolumeChartPoint[] ProjectVolumeChart(string[][] raw)
    {
        var result = new ExchangeVolumeChartPoint[raw.Length];
        for (var i = 0; i < raw.Length; i++)
        {
            var row = raw[i];
            var ms = long.Parse(row[0], System.Globalization.CultureInfo.InvariantCulture);
            var vol = decimal.Parse(row[1], System.Globalization.CultureInfo.InvariantCulture);
            result[i] = new ExchangeVolumeChartPoint
            {
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ms),
                BtcVolume = vol,
            };
        }
        return result;
    }
}
