using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class SimpleClient(HttpClient http) : ISimpleClient
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal?>>> GetPriceAsync(
        SimplePriceOptions options, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddList("ids", options.Ids)
            .AddList("names", options.Names)
            .AddList("symbols", options.Symbols)
            .AddList("vs_currencies", options.VsCurrencies)
            .Add("include_market_cap", options.IncludeMarketCap ? "true" : null)
            .Add("include_24hr_vol", options.Include24hrVol ? "true" : null)
            .Add("include_24hr_change", options.Include24hrChange ? "true" : null)
            .Add("include_last_updated_at", options.IncludeLastUpdatedAt ? "true" : null)
            .Add("precision", options.Precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, "simple/price" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadFromJsonAsync(
            CoinGeckoJsonContext.Default.DictionaryStringDictionaryStringNullableDecimal, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /simple/price.");
        return raw.ToDictionary(
            static kvp => kvp.Key,
            static kvp => (IReadOnlyDictionary<string, decimal?>)kvp.Value);
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal?>>> GetTokenPriceAsync(
        string assetPlatformId, SimpleTokenPriceOptions options, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("simple/token_price/{id}", new[] { ("id", assetPlatformId) });
        var qs = new QueryStringBuilder()
            .AddList("contract_addresses", options.ContractAddresses)
            .AddList("vs_currencies", options.VsCurrencies)
            .Add("include_market_cap", options.IncludeMarketCap ? "true" : null)
            .Add("include_24hr_vol", options.Include24hrVol ? "true" : null)
            .Add("include_24hr_change", options.Include24hrChange ? "true" : null)
            .Add("include_last_updated_at", options.IncludeLastUpdatedAt ? "true" : null)
            .Add("precision", options.Precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadFromJsonAsync(
            CoinGeckoJsonContext.Default.DictionaryStringDictionaryStringNullableDecimal, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /simple/token_price/{assetPlatformId}.");
        return raw.ToDictionary(
            static kvp => kvp.Key,
            static kvp => (IReadOnlyDictionary<string, decimal?>)kvp.Value);
    }

    public async Task<IReadOnlyList<string>> GetSupportedVsCurrenciesAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "simple/supported_vs_currencies");
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.StringArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /simple/supported_vs_currencies.");
    }
}
