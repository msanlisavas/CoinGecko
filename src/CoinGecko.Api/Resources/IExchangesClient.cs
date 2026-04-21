using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Exchanges endpoints.</summary>
public interface IExchangesClient
{
    /// <summary>List of exchanges with summary data.</summary>
    Task<IReadOnlyList<Exchange>> GetAsync(ExchangesOptions? options = null, CancellationToken ct = default);

    /// <summary>Exchange id map.</summary>
    Task<IReadOnlyList<ExchangeListItem>> GetListAsync(CancellationToken ct = default);

    /// <summary>Detail for one exchange (includes top-100 tickers).</summary>
    Task<ExchangeDetail> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Tickers traded on one exchange (paginated).</summary>
    Task<ExchangeTickers> GetTickersAsync(string id, ExchangeTickersOptions? options = null, CancellationToken ct = default);

    /// <summary>Historical BTC-volume time series for one exchange.</summary>
    Task<IReadOnlyList<ExchangeVolumeChartPoint>> GetVolumeChartAsync(string id, int days, CancellationToken ct = default);

    /// <summary>Historical BTC-volume time series for an arbitrary UNIX-seconds range.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<IReadOnlyList<ExchangeVolumeChartPoint>> GetVolumeChartRangeAsync(
        string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
}
