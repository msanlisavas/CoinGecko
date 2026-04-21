using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Derivatives endpoints.</summary>
public interface IDerivativesClient
{
    /// <summary>List of derivatives tickers.</summary>
    Task<IReadOnlyList<Derivative>> GetTickersAsync(CancellationToken ct = default);

    /// <summary>Paginated list of derivatives exchanges.</summary>
    Task<IReadOnlyList<DerivativeExchange>> GetExchangesAsync(
        DerivativeExchangesOptions? options = null, CancellationToken ct = default);

    /// <summary>Detail for a single derivatives exchange.</summary>
    Task<DerivativeExchangeDetail> GetExchangeAsync(
        string id, bool includeTickers = false, CancellationToken ct = default);

    /// <summary>ID map of derivatives exchanges.</summary>
    Task<IReadOnlyList<DerivativeExchangeListItem>> GetExchangeListAsync(CancellationToken ct = default);

    /// <summary>Auto-paginates <see cref="GetExchangesAsync"/> yielding every <see cref="DerivativeExchange"/> across all pages.</summary>
    /// <param name="options">Query options; <c>Page</c> is managed automatically.</param>
    /// <param name="ct">Cancellation token.</param>
    IAsyncEnumerable<DerivativeExchange> EnumerateExchangesAsync(
        DerivativeExchangesOptions? options = null, CancellationToken ct = default);
}
