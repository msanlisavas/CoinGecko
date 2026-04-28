using CoinGecko.Api.Internal;
using CoinGecko.Api.Models.Onchain;
using CoinGecko.Api.Serialization;
using CoinGecko.Api.Serialization.JsonApi;

namespace CoinGecko.Api.Resources;

internal sealed class OnchainClient(HttpClient http) : IOnchainClient
{
    private readonly HttpClient _http = http;

    public async Task<Network[]> GetNetworksAsync(int page = 1, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder().Add("page", page);
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/networks" + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseNetworkArray, ct).ConfigureAwait(false);
    }

    public async Task<Dex[]> GetDexesAsync(string network, int page = 1, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/dexes", new[] { ("network", network) });
        var qs = new QueryStringBuilder().Add("page", page);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseDexArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool> GetPoolAsync(string network, string address, OnchainPoolOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/pools/{address}", new[] { ("network", network), ("address", address) });
        var qs = new QueryStringBuilder().AddList("include", options?.Include, separator: ",");
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePool, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetPoolsMultiAsync(string network, IReadOnlyList<string> addresses, OnchainPoolOptions? options = null, CancellationToken ct = default)
    {
        var addressList = string.Join(",", addresses);
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/pools/multi/{addresses}", new[] { ("network", network), ("addresses", addressList) });
        var qs = new QueryStringBuilder().AddList("include", options?.Include, separator: ",");
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetTrendingPoolsAsync(OnchainTrendingPoolsOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("duration", options?.Duration);
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/networks/trending_pools" + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetTrendingPoolsByNetworkAsync(string network, OnchainTrendingPoolsOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/trending_pools", new[] { ("network", network) });
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("duration", options?.Duration);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetTopPoolsByNetworkAsync(string network, OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/pools", new[] { ("network", network) });
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("sort", options?.Sort);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetTopPoolsByDexAsync(string network, string dex, OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/dexes/{dex}/pools", new[] { ("network", network), ("dex", dex) });
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("sort", options?.Sort);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetNewPoolsAsync(OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("sort", options?.Sort);
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/networks/new_pools" + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetNewPoolsByNetworkAsync(string network, OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/new_pools", new[] { ("network", network) });
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("sort", options?.Sort);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetPoolsMegafilterAsync(OnchainMegafilterOptions options, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .AddList("networks", options.Networks, separator: ",")
            .AddList("dexes", options.Dexes, separator: ",")
            .AddList("categories", options.Categories, separator: ",")
            .Add("pool_created_hour", options.PoolCreatedHour)
            .Add("min_volume_h24_usd", options.MinVolumeH24Usd)
            .Add("min_reserve_in_usd", options.MinReserveInUsd)
            .Add("sort", options.Sort)
            .Add("page", options.Page);
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/pools/megafilter" + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetTrendingSearchPoolsAsync(CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/pools/trending_search");
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<PoolInfo> GetPoolInfoAsync(string network, string poolAddress, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/pools/{pool_address}/info", new[] { ("network", network), ("pool_address", poolAddress) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolInfo, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OnchainOhlcv>> GetPoolOhlcvAsync(string network, string poolAddress, OnchainTimeframe timeframe, OnchainOhlcvOptions? options = null, CancellationToken ct = default)
    {
        var timeframeStr = timeframe.ToString().ToLowerInvariant();
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/pools/{pool_address}/ohlcv/{timeframe}", new[] { ("network", network), ("pool_address", poolAddress), ("timeframe", timeframeStr) });
        var qs = new QueryStringBuilder()
            .Add("aggregate", options?.Aggregate)
            .Add("limit", options?.Limit)
            .AddUnixSeconds("before_timestamp", options?.BeforeTimestamp)
            .Add("currency", options?.Currency)
            .Add("token", options?.Token);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var resource = await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainOhlcvResource, ct).ConfigureAwait(false);
        var items = resource.Attributes?.OhlcvList ?? Array.Empty<decimal[]>();
        var projected = new OnchainOhlcv[items.Length];
        for (var i = 0; i < items.Length; i++)
        {
            var row = items[i];
            if (row.Length < 6) { continue; }
            projected[i] = new OnchainOhlcv(
                DateTimeOffset.FromUnixTimeSeconds((long)row[0]),
                row[1], row[2], row[3], row[4], row[5]);
        }
        return projected;
    }

    public async Task<OnchainTrade[]> GetPoolTradesAsync(string network, string poolAddress, decimal? minVolumeUsd = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/pools/{pool_address}/trades", new[] { ("network", network), ("pool_address", poolAddress) });
        var qs = new QueryStringBuilder().Add("min_volume_usd", minVolumeUsd);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTradeArray, ct).ConfigureAwait(false);
    }

    public async Task<OnchainTokenPrices> GetTokenPriceAsync(string network, IReadOnlyList<string> addresses, OnchainTokenPriceOptions? options = null, CancellationToken ct = default)
    {
        var addressList = string.Join(",", addresses);
        var path = UriTemplateExpander.Expand("onchain/simple/networks/{network}/token_price/{addresses}", new[] { ("network", network), ("addresses", addressList) });
        var qs = new QueryStringBuilder()
            .Add("include_market_cap", options?.IncludeMarketCap == true ? "true" : null)
            .Add("include_24hr_vol", options?.Include24hrVol == true ? "true" : null)
            .Add("include_24hr_price_change", options?.Include24hrPriceChange == true ? "true" : null)
            .Add("include_total_reserve_in_usd", options?.IncludeTotalReserveInUsd == true ? "true" : null);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTokenPrices, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> SearchPoolsAsync(string query, string? network = null, int page = 1, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("query", query)
            .Add("network", network)
            .Add("page", page);
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/search/pools" + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<OnchainToken> GetTokenAsync(string network, string address, OnchainPoolOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/{address}", new[] { ("network", network), ("address", address) });
        var qs = new QueryStringBuilder().AddList("include", options?.Include, separator: ",");
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainToken, ct).ConfigureAwait(false);
    }

    public async Task<OnchainToken[]> GetTokensMultiAsync(string network, IReadOnlyList<string> addresses, OnchainPoolOptions? options = null, CancellationToken ct = default)
    {
        var addressList = string.Join(",", addresses);
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/multi/{addresses}", new[] { ("network", network), ("addresses", addressList) });
        var qs = new QueryStringBuilder().AddList("include", options?.Include, separator: ",");
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTokenArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetPoolsByTokenAsync(string network, string tokenAddress, OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/{token_address}/pools", new[] { ("network", network), ("token_address", tokenAddress) });
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("sort", options?.Sort);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public async Task<OnchainTokenInfo> GetTokenInfoAsync(string network, string address, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/{address}/info", new[] { ("network", network), ("address", address) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTokenInfo, ct).ConfigureAwait(false);
    }

    public async Task<OnchainTokenInfo[]> GetRecentlyUpdatedTokensAsync(string? include = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder().Add("include", include);
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/tokens/info_recently_updated" + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTokenInfoArray, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OnchainOhlcv>> GetTokenOhlcvAsync(string network, string tokenAddress, OnchainTimeframe timeframe, OnchainOhlcvOptions? options = null, CancellationToken ct = default)
    {
        var timeframeStr = timeframe.ToString().ToLowerInvariant();
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/{token_address}/ohlcv/{timeframe}", new[] { ("network", network), ("token_address", tokenAddress), ("timeframe", timeframeStr) });
        var qs = new QueryStringBuilder()
            .Add("aggregate", options?.Aggregate)
            .Add("limit", options?.Limit)
            .AddUnixSeconds("before_timestamp", options?.BeforeTimestamp)
            .Add("currency", options?.Currency)
            .Add("token", options?.Token);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var resource = await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainOhlcvResource, ct).ConfigureAwait(false);
        var items = resource.Attributes?.OhlcvList ?? Array.Empty<decimal[]>();
        var projected = new OnchainOhlcv[items.Length];
        for (var i = 0; i < items.Length; i++)
        {
            var row = items[i];
            if (row.Length < 6) { continue; }
            projected[i] = new OnchainOhlcv(
                DateTimeOffset.FromUnixTimeSeconds((long)row[0]),
                row[1], row[2], row[3], row[4], row[5]);
        }
        return projected;
    }

    public async Task<OnchainTrade[]> GetTokenTradesAsync(string network, string tokenAddress, decimal? minVolumeUsd = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/{token_address}/trades", new[] { ("network", network), ("token_address", tokenAddress) });
        var qs = new QueryStringBuilder().Add("min_volume_usd", minVolumeUsd);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTradeArray, ct).ConfigureAwait(false);
    }

    public async Task<OnchainTopTrader[]> GetTopTradersAsync(string networkId, string tokenAddress, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network_id}/tokens/{token_address}/top_traders", new[] { ("network_id", networkId), ("token_address", tokenAddress) });
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTopTraderArray, ct).ConfigureAwait(false);
    }

    public async Task<OnchainTopHolders> GetTopHoldersAsync(string network, string address, OnchainTopHoldersOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/{address}/top_holders", new[] { ("network", network), ("address", address) });
        var qs = new QueryStringBuilder()
            .Add("holders", options?.Holders)
            .Add("include_pnl_details", options?.IncludePnlDetails);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainTopHolders, ct).ConfigureAwait(false);
    }

    public async Task<OnchainHoldersChart> GetHoldersChartAsync(string network, string tokenAddress, int days, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/networks/{network}/tokens/{token_address}/holders_chart", new[] { ("network", network), ("token_address", tokenAddress) });
        var qs = new QueryStringBuilder().Add("days", days);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainHoldersChart, ct).ConfigureAwait(false);
    }

    public async Task<OnchainCategory[]> GetCategoriesAsync(OnchainCategoriesOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("page", options?.Page)
            .Add("sort", options?.Sort);
        using var req = new HttpRequestMessage(HttpMethod.Get, "onchain/categories" + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponseOnchainCategoryArray, ct).ConfigureAwait(false);
    }

    public async Task<Pool[]> GetCategoryPoolsAsync(string categoryId, OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var path = UriTemplateExpander.Expand("onchain/categories/{category_id}/pools", new[] { ("category_id", categoryId) });
        var qs = new QueryStringBuilder()
            .AddList("include", options?.Include, separator: ",")
            .Add("page", options?.Page)
            .Add("sort", options?.Sort);
        using var req = new HttpRequestMessage(HttpMethod.Get, path + qs);
        req.Options.Set(CoinGeckoRequestOptions.Envelope, ResponseEnvelope.JsonApi);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Basic);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await JsonApiUnwrap.ReadDataAsync(resp.Content, CoinGeckoJsonContext.Default.JsonApiResponsePoolArray, ct).ConfigureAwait(false);
    }

    public IAsyncEnumerable<Pool> EnumerateTrendingPoolsAsync(
        OnchainTrendingPoolsOptions? options = null, CancellationToken ct = default)
    {
        var baseOptions = options ?? new OnchainTrendingPoolsOptions();
        // GeckoTerminal returns 20 pools per page (fixed).
        return PaginationHelper.EnumerateAsync<Pool>(
            fetchPage: async (page, c) =>
                (IReadOnlyList<Pool>)await GetTrendingPoolsAsync(baseOptions with { Page = page }, c).ConfigureAwait(false),
            perPage: 20,
            ct: ct);
    }

    public IAsyncEnumerable<Pool> EnumerateTopPoolsByNetworkAsync(
        string network, OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var baseOptions = options ?? new OnchainPoolsListOptions();
        // GeckoTerminal returns 20 pools per page (fixed).
        return PaginationHelper.EnumerateAsync<Pool>(
            fetchPage: async (page, c) =>
                (IReadOnlyList<Pool>)await GetTopPoolsByNetworkAsync(network, baseOptions with { Page = page }, c).ConfigureAwait(false),
            perPage: 20,
            ct: ct);
    }

    public IAsyncEnumerable<Pool> EnumerateNewPoolsAsync(
        OnchainPoolsListOptions? options = null, CancellationToken ct = default)
    {
        var baseOptions = options ?? new OnchainPoolsListOptions();
        // GeckoTerminal returns 20 pools per page (fixed).
        return PaginationHelper.EnumerateAsync<Pool>(
            fetchPage: async (page, c) =>
                (IReadOnlyList<Pool>)await GetNewPoolsAsync(baseOptions with { Page = page }, c).ConfigureAwait(false),
            perPage: 20,
            ct: ct);
    }
}
