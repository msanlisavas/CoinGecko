using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Global market endpoints.</summary>
public interface IGlobalClient
{
    /// <summary>Global market snapshot.</summary>
    Task<GlobalMarket> GetAsync(CancellationToken ct = default);

    /// <summary>DeFi market snapshot.</summary>
    Task<DefiGlobal> GetDefiAsync(CancellationToken ct = default);

    /// <summary>Historical global market cap + volume time series.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<IReadOnlyList<GlobalMarketCapPoint>> GetMarketCapChartAsync(
        int days, string vsCurrency = "usd", CancellationToken ct = default);
}
