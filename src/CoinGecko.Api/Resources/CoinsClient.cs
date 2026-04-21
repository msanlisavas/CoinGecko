using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class CoinsClient(HttpClient http) : ICoinsClient
{
    private readonly HttpClient _http = http;

    public async Task<IReadOnlyList<CoinListItem>> GetListAsync(
        bool includePlatform = false, string? status = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("include_platform", includePlatform ? "true" : null)
            .Add("status", status);
        using var req = new HttpRequestMessage(HttpMethod.Get, "coins/list" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CoinListItemArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /coins/list.");
    }

    public async Task<IReadOnlyList<CoinMarket>> GetMarketsAsync(
        string vsCurrency, CoinMarketsOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .AddList("ids", options?.Ids)
            .Add("category", options?.Category)
            .AddEnum("order", options?.Order ?? CoinMarketsOrder.MarketCapDesc)
            .Add("per_page", options?.PerPage)
            .Add("page", options?.Page)
            .Add("sparkline", options?.Sparkline == true ? "true" : null)
            .AddEnumList("price_change_percentage", options?.PriceChangePercentage)
            .Add("locale", options?.Locale)
            .Add("precision", options?.Precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, "coins/markets" + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CoinMarketArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /coins/markets.");
    }

    public async Task<Coin> GetAsync(string id, CoinDetailOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .Add("localization", options?.Localization == true ? "true" : "false")
            .Add("tickers", options?.Tickers == false ? "false" : null)
            .Add("market_data", options?.MarketData == false ? "false" : null)
            .Add("community_data", options?.CommunityData == false ? "false" : null)
            .Add("developer_data", options?.DeveloperData == false ? "false" : null)
            .Add("sparkline", options?.Sparkline == true ? "true" : null)
            .Add("include_categories_details", options?.IncludeCategoriesDetails == true ? "true" : null)
            .Add("dex_pair_format", options?.DexPairFormat);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.Coin, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}.");
    }

    public async Task<CoinTickers> GetTickersAsync(string id, CoinTickersOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/tickers", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddList("exchange_ids", options?.ExchangeIds)
            .Add("include_exchange_logo", options?.IncludeExchangeLogo == true ? "true" : null)
            .Add("page", options?.Page)
            .Add("order", options?.Order)
            .Add("depth", options?.Depth == true ? "true" : null)
            .Add("dex_pair_format", options?.DexPairFormat);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CoinTickers, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/tickers.");
    }

    public async Task<CoinHistory> GetHistoryAsync(string id, DateOnly date, bool localization = false, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/history", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddCoinGeckoDate("date", date)
            .Add("localization", localization ? "true" : "false");
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CoinHistory, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/history.");
    }

    public async Task<MarketChart> GetMarketChartAsync(
        string id, string vsCurrency, MarketChartRange days,
        string? interval = null, string? precision = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/market_chart", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .AddEnum("days", days)
            .Add("interval", interval)
            .Add("precision", precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.MarketChart, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/market_chart.");
    }

    public async Task<MarketChart> GetMarketChartRangeAsync(
        string id, string vsCurrency, DateTimeOffset from, DateTimeOffset to,
        string? precision = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/market_chart/range", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .AddUnixSeconds("from", from)
            .AddUnixSeconds("to", to)
            .Add("precision", precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.MarketChart, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/market_chart/range.");
    }

    public async Task<IReadOnlyList<CoinOhlc>> GetOhlcAsync(
        string id, string vsCurrency, MarketChartRange days,
        string? interval = null, string? precision = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/ohlc", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .AddEnum("days", days)
            .Add("interval", interval)
            .Add("precision", precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CoinOhlcArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/ohlc.");
    }

    public async Task<IReadOnlyList<CoinOhlc>> GetOhlcRangeAsync(
        string id, string vsCurrency, DateTimeOffset from, DateTimeOffset to,
        string? interval = null, string? precision = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/ohlc/range", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .AddUnixSeconds("from", from)
            .AddUnixSeconds("to", to)
            .Add("interval", interval)
            .Add("precision", precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.CoinOhlcArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/ohlc/range.");
    }

    public async Task<IReadOnlyList<SupplyPoint>> GetCirculatingSupplyChartAsync(
        string id, MarketChartRange days, string? interval = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/circulating_supply_chart", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddEnum("days", days)
            .Add("interval", interval);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.SupplyPointArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/circulating_supply_chart.");
    }

    public async Task<IReadOnlyList<SupplyPoint>> GetCirculatingSupplyChartRangeAsync(
        string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/circulating_supply_chart/range", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddUnixSeconds("from", from)
            .AddUnixSeconds("to", to);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.SupplyPointArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/circulating_supply_chart/range.");
    }

    public async Task<IReadOnlyList<SupplyPoint>> GetTotalSupplyChartAsync(
        string id, MarketChartRange days, string? interval = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/total_supply_chart", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddEnum("days", days)
            .Add("interval", interval);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.SupplyPointArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/total_supply_chart.");
    }

    public async Task<IReadOnlyList<SupplyPoint>> GetTotalSupplyChartRangeAsync(
        string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("coins/{id}/total_supply_chart/range", new[] { ("id", id) });
        var qs = new QueryStringBuilder()
            .AddUnixSeconds("from", from)
            .AddUnixSeconds("to", to);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.SupplyPointArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/total_supply_chart/range.");
    }

    public async Task<Coin> GetByContractAsync(string id, string contractAddress, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand(
            "coins/{id}/contract/{contract_address}",
            new[] { ("id", id), ("contract_address", contractAddress) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.Coin, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/contract/{contractAddress}.");
    }

    public async Task<MarketChart> GetContractMarketChartAsync(
        string id, string contractAddress, string vsCurrency, MarketChartRange days,
        string? precision = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand(
            "coins/{id}/contract/{contract_address}/market_chart",
            new[] { ("id", id), ("contract_address", contractAddress) });
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .AddEnum("days", days)
            .Add("precision", precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.MarketChart, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/contract/{contractAddress}/market_chart.");
    }

    public async Task<MarketChart> GetContractMarketChartRangeAsync(
        string id, string contractAddress, string vsCurrency,
        DateTimeOffset from, DateTimeOffset to,
        string? precision = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand(
            "coins/{id}/contract/{contract_address}/market_chart/range",
            new[] { ("id", id), ("contract_address", contractAddress) });
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .AddUnixSeconds("from", from)
            .AddUnixSeconds("to", to)
            .Add("precision", precision);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs.ToString());
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.MarketChart, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"CoinGecko returned empty body for /coins/{id}/contract/{contractAddress}/market_chart/range.");
    }

    public async Task<TopGainersLosers> GetTopGainersLosersAsync(
        string vsCurrency, string? duration = null, string? topCoins = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("vs_currency", vsCurrency)
            .Add("duration", duration)
            .Add("top_coins", topCoins);
        using var req = new HttpRequestMessage(HttpMethod.Get, "coins/top_gainers_losers" + qs.ToString());
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.TopGainersLosers, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /coins/top_gainers_losers.");
    }

    public async Task<IReadOnlyList<NewCoinListing>> GetNewListingsAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "coins/list/new");
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.NewCoinListingArray, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko returned empty body for /coins/list/new.");
    }
}
