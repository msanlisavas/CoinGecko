using CoinGecko.Api.Models.Onchain;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Onchain (GeckoTerminal) endpoints.</summary>
public interface IOnchainClient
{
    /// <summary>Returns all supported blockchain networks. <c>GET /onchain/networks</c></summary>
    Task<Network[]> GetNetworksAsync(int page = 1, CancellationToken ct = default);

    /// <summary>Returns DEXes on a given network. <c>GET /onchain/networks/{network}/dexes</c></summary>
    Task<Dex[]> GetDexesAsync(string network, int page = 1, CancellationToken ct = default);

    /// <summary>Returns a single pool by address. <c>GET /onchain/networks/{network}/pools/{address}</c></summary>
    Task<Pool> GetPoolAsync(string network, string address, OnchainPoolOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns multiple pools by address. <c>GET /onchain/networks/{network}/pools/multi/{addresses}</c></summary>
    Task<Pool[]> GetPoolsMultiAsync(string network, IReadOnlyList<string> addresses, OnchainPoolOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns global trending pools. <c>GET /onchain/networks/trending_pools</c></summary>
    Task<Pool[]> GetTrendingPoolsAsync(OnchainTrendingPoolsOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns trending pools for a specific network. <c>GET /onchain/networks/{network}/trending_pools</c></summary>
    Task<Pool[]> GetTrendingPoolsByNetworkAsync(string network, OnchainTrendingPoolsOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns top pools for a network. <c>GET /onchain/networks/{network}/pools</c></summary>
    Task<Pool[]> GetTopPoolsByNetworkAsync(string network, OnchainPoolsListOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns top pools for a DEX on a network. <c>GET /onchain/networks/{network}/dexes/{dex}/pools</c></summary>
    Task<Pool[]> GetTopPoolsByDexAsync(string network, string dex, OnchainPoolsListOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns newly created pools across all networks. <c>GET /onchain/networks/new_pools</c></summary>
    Task<Pool[]> GetNewPoolsAsync(OnchainPoolsListOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns newly created pools on a specific network. <c>GET /onchain/networks/{network}/new_pools</c></summary>
    Task<Pool[]> GetNewPoolsByNetworkAsync(string network, OnchainPoolsListOptions? options = null, CancellationToken ct = default);

    /// <summary>Advanced pool filter (Analyst+). <c>GET /onchain/pools/megafilter</c></summary>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<Pool[]> GetPoolsMegafilterAsync(OnchainMegafilterOptions options, CancellationToken ct = default);

    /// <summary>Returns trending search pools (Basic+). <c>GET /onchain/pools/trending_search</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<Pool[]> GetTrendingSearchPoolsAsync(CancellationToken ct = default);

    /// <summary>Returns token info for both sides of a pool. <c>GET /onchain/networks/{network}/pools/{poolAddress}/info</c></summary>
    Task<PoolInfo> GetPoolInfoAsync(string network, string poolAddress, CancellationToken ct = default);

    /// <summary>Returns OHLCV data for a pool. <c>GET /onchain/networks/{network}/pools/{poolAddress}/ohlcv/{timeframe}</c></summary>
    Task<IReadOnlyList<OnchainOhlcv>> GetPoolOhlcvAsync(string network, string poolAddress, OnchainTimeframe timeframe, OnchainOhlcvOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns recent trades for a pool. <c>GET /onchain/networks/{network}/pools/{poolAddress}/trades</c></summary>
    Task<OnchainTrade[]> GetPoolTradesAsync(string network, string poolAddress, decimal? minVolumeUsd = null, CancellationToken ct = default);

    /// <summary>Returns token prices on a network. <c>GET /onchain/simple/networks/{network}/token_price/{addresses}</c></summary>
    Task<OnchainTokenPrices> GetTokenPriceAsync(string network, IReadOnlyList<string> addresses, OnchainTokenPriceOptions? options = null, CancellationToken ct = default);

    /// <summary>Searches pools by query. <c>GET /onchain/search/pools</c></summary>
    Task<Pool[]> SearchPoolsAsync(string query, string? network = null, int page = 1, CancellationToken ct = default);

    /// <summary>Returns a single token by address. <c>GET /onchain/networks/{network}/tokens/{address}</c></summary>
    Task<OnchainToken> GetTokenAsync(string network, string address, OnchainPoolOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns multiple tokens by address. <c>GET /onchain/networks/{network}/tokens/multi/{addresses}</c></summary>
    Task<OnchainToken[]> GetTokensMultiAsync(string network, IReadOnlyList<string> addresses, OnchainPoolOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns pools for a token. <c>GET /onchain/networks/{network}/tokens/{tokenAddress}/pools</c></summary>
    Task<Pool[]> GetPoolsByTokenAsync(string network, string tokenAddress, OnchainPoolsListOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns extended token info. <c>GET /onchain/networks/{network}/tokens/{address}/info</c></summary>
    Task<OnchainTokenInfo> GetTokenInfoAsync(string network, string address, CancellationToken ct = default);

    /// <summary>Returns recently updated token info. <c>GET /onchain/tokens/info_recently_updated</c></summary>
    Task<OnchainTokenInfo[]> GetRecentlyUpdatedTokensAsync(string? include = null, CancellationToken ct = default);

    /// <summary>Returns OHLCV data for a token. <c>GET /onchain/networks/{network}/tokens/{tokenAddress}/ohlcv/{timeframe}</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<IReadOnlyList<OnchainOhlcv>> GetTokenOhlcvAsync(string network, string tokenAddress, OnchainTimeframe timeframe, OnchainOhlcvOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns recent trades for a token. <c>GET /onchain/networks/{network}/tokens/{tokenAddress}/trades</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<OnchainTrade[]> GetTokenTradesAsync(string network, string tokenAddress, decimal? minVolumeUsd = null, CancellationToken ct = default);

    /// <summary>Returns top traders for a token. <c>GET /onchain/networks/{networkId}/tokens/{tokenAddress}/top_traders</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<OnchainTopTrader[]> GetTopTradersAsync(string networkId, string tokenAddress, CancellationToken ct = default);

    /// <summary>Returns top holders for a token, optionally with PnL details. <c>GET /onchain/networks/{network}/tokens/{address}/top_holders</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<OnchainTopHolders> GetTopHoldersAsync(string network, string address, OnchainTopHoldersOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns holder count chart for a token. <c>GET /onchain/networks/{network}/tokens/{tokenAddress}/holders_chart</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<OnchainHoldersChart> GetHoldersChartAsync(string network, string tokenAddress, int days, CancellationToken ct = default);

    /// <summary>Returns onchain categories. <c>GET /onchain/categories</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<OnchainCategory[]> GetCategoriesAsync(OnchainCategoriesOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns pools for an onchain category. <c>GET /onchain/categories/{categoryId}/pools</c></summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<Pool[]> GetCategoryPoolsAsync(string categoryId, OnchainPoolsListOptions? options = null, CancellationToken ct = default);

    /// <summary>Auto-paginates <see cref="GetTrendingPoolsAsync"/> yielding every <see cref="Pool"/> across all pages.</summary>
    /// <param name="options">Query options; <c>Page</c> is managed automatically.</param>
    /// <param name="ct">Cancellation token.</param>
    IAsyncEnumerable<Pool> EnumerateTrendingPoolsAsync(
        OnchainTrendingPoolsOptions? options = null, CancellationToken ct = default);

    /// <summary>Auto-paginates <see cref="GetTopPoolsByNetworkAsync"/> yielding every <see cref="Pool"/> across all pages.</summary>
    /// <param name="network">Blockchain network slug.</param>
    /// <param name="options">Query options; <c>Page</c> is managed automatically.</param>
    /// <param name="ct">Cancellation token.</param>
    IAsyncEnumerable<Pool> EnumerateTopPoolsByNetworkAsync(
        string network, OnchainPoolsListOptions? options = null, CancellationToken ct = default);

    /// <summary>Auto-paginates <see cref="GetNewPoolsAsync"/> yielding every <see cref="Pool"/> across all pages.</summary>
    /// <param name="options">Query options; <c>Page</c> is managed automatically.</param>
    /// <param name="ct">Cancellation token.</param>
    IAsyncEnumerable<Pool> EnumerateNewPoolsAsync(
        OnchainPoolsListOptions? options = null, CancellationToken ct = default);
}
