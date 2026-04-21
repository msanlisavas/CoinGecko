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
}
