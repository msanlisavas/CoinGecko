namespace CoinGecko.Api.Models;

/// <summary>A single OHLC candle from <c>/coins/{id}/ohlc</c>.</summary>
public sealed record CoinOhlc(
    DateTimeOffset Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close);
