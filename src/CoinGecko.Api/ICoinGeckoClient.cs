using CoinGecko.Api.Resources;

namespace CoinGecko.Api;

/// <summary>Root entry point. Exposes one sub-client per CoinGecko resource group.</summary>
public interface ICoinGeckoClient
{
    /// <summary>Gets the sub-client for the Coins resource group.</summary>
    ICoinsClient Coins { get; }

    /// <summary>Gets the sub-client for the NFTs resource group.</summary>
    INftsClient Nfts { get; }

    /// <summary>Gets the sub-client for the Exchanges resource group.</summary>
    IExchangesClient Exchanges { get; }

    /// <summary>Gets the sub-client for the Derivatives resource group.</summary>
    IDerivativesClient Derivatives { get; }

    /// <summary>Gets the sub-client for the Categories resource group.</summary>
    ICategoriesClient Categories { get; }

    /// <summary>Gets the sub-client for the Asset Platforms resource group.</summary>
    IAssetPlatformsClient AssetPlatforms { get; }

    /// <summary>Gets the sub-client for the Companies resource group.</summary>
    ICompaniesClient Companies { get; }

    /// <summary>Gets the sub-client for the Simple resource group.</summary>
    ISimpleClient Simple { get; }

    /// <summary>Gets the sub-client for the Global resource group.</summary>
    IGlobalClient Global { get; }

    /// <summary>Gets the sub-client for the Search resource group.</summary>
    ISearchClient Search { get; }

    /// <summary>Gets the sub-client for the Trending resource group.</summary>
    ITrendingClient Trending { get; }

    /// <summary>Gets the sub-client for the Onchain (GeckoTerminal) resource group.</summary>
    IOnchainClient Onchain { get; }

    /// <summary>Gets the sub-client for the Key resource group.</summary>
    IKeyClient Key { get; }

    /// <summary>Gets the sub-client for the Crypto News resource group (paid plans).</summary>
    INewsClient News { get; }

    /// <summary>Gets the sub-client for the Ping resource group.</summary>
    IPingClient Ping { get; }
}
