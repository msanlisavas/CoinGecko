using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's NFTs endpoints.</summary>
public interface INftsClient
{
    /// <summary>Paginated NFT id map.</summary>
    Task<IReadOnlyList<NftListItem>> GetListAsync(
        int perPage = 100, int page = 1, CancellationToken ct = default);

    /// <summary>Detail for one NFT collection by id.</summary>
    Task<Nft> GetAsync(string id, CancellationToken ct = default);

    /// <summary>NFT collection by on-chain contract address.</summary>
    Task<Nft> GetByContractAsync(string assetPlatformId, string contractAddress, CancellationToken ct = default);

    /// <summary>Paginated NFTs with market data.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<IReadOnlyList<NftMarket>> GetMarketsAsync(
        NftMarketsOptions? options = null, CancellationToken ct = default);

    /// <summary>Market chart for one NFT collection.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<IReadOnlyList<NftMarketChartPoint>> GetMarketChartAsync(
        string id, int days, CancellationToken ct = default);

    /// <summary>Tickers across NFT marketplaces for one collection.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<NftTickers> GetTickersAsync(string id, CancellationToken ct = default);
}
