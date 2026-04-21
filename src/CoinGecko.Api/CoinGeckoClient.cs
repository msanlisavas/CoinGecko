using CoinGecko.Api.Resources;

namespace CoinGecko.Api;

internal sealed class CoinGeckoClient(
    ICoinsClient coins,
    INftsClient nfts,
    IExchangesClient exchanges,
    IDerivativesClient derivatives,
    ICategoriesClient categories,
    IAssetPlatformsClient assetPlatforms,
    ICompaniesClient companies,
    ISimpleClient simple,
    IGlobalClient global,
    ISearchClient search,
    ITrendingClient trending,
    IOnchainClient onchain,
    IKeyClient key,
    IPingClient ping) : ICoinGeckoClient
{
    public ICoinsClient Coins { get; } = coins;
    public INftsClient Nfts { get; } = nfts;
    public IExchangesClient Exchanges { get; } = exchanges;
    public IDerivativesClient Derivatives { get; } = derivatives;
    public ICategoriesClient Categories { get; } = categories;
    public IAssetPlatformsClient AssetPlatforms { get; } = assetPlatforms;
    public ICompaniesClient Companies { get; } = companies;
    public ISimpleClient Simple { get; } = simple;
    public IGlobalClient Global { get; } = global;
    public ISearchClient Search { get; } = search;
    public ITrendingClient Trending { get; } = trending;
    public IOnchainClient Onchain { get; } = onchain;
    public IKeyClient Key { get; } = key;
    public IPingClient Ping { get; } = ping;
}
