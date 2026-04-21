using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Simple endpoints.</summary>
public interface ISimpleClient
{
    /// <summary>Current prices for one or more coins in one or more quote currencies.</summary>
    Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal?>>> GetPriceAsync(
        SimplePriceOptions options, CancellationToken ct = default);

    /// <summary>Current prices for ERC-20/SPL/etc. tokens by contract address.</summary>
    Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal?>>> GetTokenPriceAsync(
        string assetPlatformId, SimpleTokenPriceOptions options, CancellationToken ct = default);

    /// <summary>List of supported vs_currency values.</summary>
    Task<IReadOnlyList<string>> GetSupportedVsCurrenciesAsync(CancellationToken ct = default);
}
