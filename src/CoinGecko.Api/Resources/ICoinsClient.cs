using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's <c>/coins</c> endpoints.</summary>
public interface ICoinsClient
{
    /// <summary>Returns the list of all supported coins with id, symbol, and name.</summary>
    /// <param name="includePlatform">When <c>true</c>, includes platform contract addresses.</param>
    /// <param name="status">Filter by status (e.g. <c>"active"</c>).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<CoinListItem>> GetListAsync(
        bool includePlatform = false, string? status = null, CancellationToken ct = default);

    /// <summary>Returns coin market data for the given options.</summary>
    /// <param name="vsCurrency">The target currency (e.g. <c>"usd"</c>).</param>
    /// <param name="options">Additional query options.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<CoinMarket>> GetMarketsAsync(
        string vsCurrency, CoinMarketsOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns full details for a single coin.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="options">Options controlling which sections to include.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Coin> GetAsync(string id, CoinDetailOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns ticker data for a coin across all exchanges.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="options">Options for filtering and pagination.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<CoinTickers> GetTickersAsync(string id, CoinTickersOptions? options = null, CancellationToken ct = default);

    /// <summary>Returns historical data for a coin on a specific date.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="date">The date to query (formatted as <c>dd-MM-yyyy</c>).</param>
    /// <param name="localization">Whether to include localized language data.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<CoinHistory> GetHistoryAsync(string id, DateOnly date, bool localization = false, CancellationToken ct = default);

    /// <summary>Returns market chart data for a coin over the given range.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="vsCurrency">The target currency.</param>
    /// <param name="days">Number of days of data (use <see cref="MarketChartRange.Max"/> for full history).</param>
    /// <param name="interval">Data interval override (e.g. <c>"daily"</c>). Null uses auto.</param>
    /// <param name="precision">Decimal precision for price values.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<MarketChart> GetMarketChartAsync(
        string id, string vsCurrency, MarketChartRange days,
        string? interval = null, string? precision = null, CancellationToken ct = default);

    /// <summary>Returns market chart data for a coin between two timestamps.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="vsCurrency">The target currency.</param>
    /// <param name="from">Start of range (inclusive).</param>
    /// <param name="to">End of range (inclusive).</param>
    /// <param name="precision">Decimal precision for price values.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<MarketChart> GetMarketChartRangeAsync(
        string id, string vsCurrency, DateTimeOffset from, DateTimeOffset to,
        string? precision = null, CancellationToken ct = default);

    /// <summary>Returns OHLC candlestick data for a coin.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="vsCurrency">The target currency.</param>
    /// <param name="days">Number of days (use <see cref="MarketChartRange.Max"/> for full history).</param>
    /// <param name="interval">OHLC interval override.</param>
    /// <param name="precision">Decimal precision.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<CoinOhlc>> GetOhlcAsync(
        string id, string vsCurrency, MarketChartRange days,
        string? interval = null, string? precision = null, CancellationToken ct = default);

    /// <summary>Returns OHLC candlestick data for a coin between two timestamps.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="vsCurrency">The target currency.</param>
    /// <param name="from">Start of range.</param>
    /// <param name="to">End of range.</param>
    /// <param name="interval">OHLC interval.</param>
    /// <param name="precision">Decimal precision.</param>
    /// <param name="ct">Cancellation token.</param>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<IReadOnlyList<CoinOhlc>> GetOhlcRangeAsync(
        string id, string vsCurrency, DateTimeOffset from, DateTimeOffset to,
        string? interval = null, string? precision = null, CancellationToken ct = default);

    /// <summary>Returns circulating supply chart data for a coin.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="days">Number of days.</param>
    /// <param name="interval">Data interval override.</param>
    /// <param name="ct">Cancellation token.</param>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<IReadOnlyList<SupplyPoint>> GetCirculatingSupplyChartAsync(
        string id, MarketChartRange days, string? interval = null, CancellationToken ct = default);

    /// <summary>Returns circulating supply chart data for a coin between two timestamps.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="from">Start of range.</param>
    /// <param name="to">End of range.</param>
    /// <param name="ct">Cancellation token.</param>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<IReadOnlyList<SupplyPoint>> GetCirculatingSupplyChartRangeAsync(
        string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    /// <summary>Returns total supply chart data for a coin.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="days">Number of days.</param>
    /// <param name="interval">Data interval override.</param>
    /// <param name="ct">Cancellation token.</param>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<IReadOnlyList<SupplyPoint>> GetTotalSupplyChartAsync(
        string id, MarketChartRange days, string? interval = null, CancellationToken ct = default);

    /// <summary>Returns total supply chart data for a coin between two timestamps.</summary>
    /// <param name="id">CoinGecko coin id.</param>
    /// <param name="from">Start of range.</param>
    /// <param name="to">End of range.</param>
    /// <param name="ct">Cancellation token.</param>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<IReadOnlyList<SupplyPoint>> GetTotalSupplyChartRangeAsync(
        string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    /// <summary>Returns coin data by its contract address on a given platform.</summary>
    /// <param name="id">Platform id (e.g. <c>"ethereum"</c>).</param>
    /// <param name="contractAddress">Token contract address.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Coin> GetByContractAsync(string id, string contractAddress, CancellationToken ct = default);

    /// <summary>Returns market chart data for a token by its contract address.</summary>
    /// <param name="id">Platform id.</param>
    /// <param name="contractAddress">Token contract address.</param>
    /// <param name="vsCurrency">The target currency.</param>
    /// <param name="days">Number of days.</param>
    /// <param name="precision">Decimal precision.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<MarketChart> GetContractMarketChartAsync(
        string id, string contractAddress, string vsCurrency, MarketChartRange days,
        string? precision = null, CancellationToken ct = default);

    /// <summary>Returns market chart data for a token by contract address between two timestamps.</summary>
    /// <param name="id">Platform id.</param>
    /// <param name="contractAddress">Token contract address.</param>
    /// <param name="vsCurrency">The target currency.</param>
    /// <param name="from">Start of range.</param>
    /// <param name="to">End of range.</param>
    /// <param name="precision">Decimal precision.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<MarketChart> GetContractMarketChartRangeAsync(
        string id, string contractAddress, string vsCurrency,
        DateTimeOffset from, DateTimeOffset to,
        string? precision = null, CancellationToken ct = default);

    /// <summary>Returns the top gainers and losers for a given currency and duration.</summary>
    /// <param name="vsCurrency">The target currency.</param>
    /// <param name="duration">Duration window (e.g. <c>"24h"</c>, <c>"7d"</c>).</param>
    /// <param name="topCoins">Number of top coins to include.</param>
    /// <param name="ct">Cancellation token.</param>
    [RequiresPlan(CoinGeckoPlan.Analyst)]
    Task<TopGainersLosers> GetTopGainersLosersAsync(
        string vsCurrency, string? duration = null, string? topCoins = null, CancellationToken ct = default);

    /// <summary>Returns recently listed coins.</summary>
    /// <param name="ct">Cancellation token.</param>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<IReadOnlyList<NewCoinListing>> GetNewListingsAsync(CancellationToken ct = default);
}
