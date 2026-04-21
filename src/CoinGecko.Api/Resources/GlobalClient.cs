using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class GlobalClient(HttpClient http) : IGlobalClient
{
    private readonly HttpClient _http = http;

    public async Task<GlobalMarket> GetAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "global");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var envelope = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.GlobalMarketEnvelope, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /global.");
        return envelope.Data ?? throw new InvalidOperationException("CoinGecko /global response missing 'data' field.");
    }

    public async Task<DefiGlobal> GetDefiAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "global/decentralized_finance_defi");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var envelope = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.DefiGlobalEnvelope, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /global/decentralized_finance_defi.");
        return envelope.Data ?? throw new InvalidOperationException("CoinGecko /global/decentralized_finance_defi response missing 'data' field.");
    }

    public async Task<IReadOnlyList<GlobalMarketCapPoint>> GetMarketCapChartAsync(
        int days, string vsCurrency = "usd", CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("days", days)
            .Add("vs_currency", vsCurrency);
        using var req = new HttpRequestMessage(HttpMethod.Get, "global/market_cap_chart" + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var envelope = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.MarketCapChartEnvelope, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /global/market_cap_chart.");
        var chart = envelope.MarketCapChart;
        var caps = chart?.MarketCap ?? [];
        var vols = chart?.Volume ?? [];
        var count = Math.Max(caps.Length, vols.Length);
        var result = new GlobalMarketCapPoint[count];
        for (var i = 0; i < count; i++)
        {
            var ts = i < caps.Length && caps[i].Length > 0
                ? DateTimeOffset.FromUnixTimeMilliseconds((long)caps[i][0])
                : DateTimeOffset.MinValue;
            var cap = i < caps.Length && caps[i].Length > 1 ? caps[i][1] : 0m;
            var vol = i < vols.Length && vols[i].Length > 1 ? vols[i][1] : 0m;
            result[i] = new GlobalMarketCapPoint { Timestamp = ts, MarketCap = cap, Volume24h = vol };
        }

        return result;
    }

    // Internal envelope types for /global/market_cap_chart deserialization.
    internal sealed class MarketCapChartEnvelope
    {
        [JsonPropertyName("market_cap_chart")] public MarketCapChartData? MarketCapChart { get; init; }
    }

    internal sealed class MarketCapChartData
    {
        [JsonPropertyName("market_cap")] public decimal[][]? MarketCap { get; init; }
        [JsonPropertyName("volume")]     public decimal[][]? Volume { get; init; }
    }
}
