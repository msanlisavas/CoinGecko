using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>G3 channel tick — DEX pool OHLCV candle.</summary>
public sealed class OnchainOhlcvCandle
{
    /// <summary>Channel code (always <c>"G3"</c>).</summary>
    [JsonPropertyName("ch")] public string? ChannelCode { get; init; }
    /// <summary>Network id.</summary>
    [JsonPropertyName("n")] public string? NetworkId { get; init; }
    /// <summary>Pool address.</summary>
    [JsonPropertyName("pa")] public string? PoolAddress { get; init; }
    /// <summary>Token side — <c>"base"</c> or <c>"quote"</c>.</summary>
    [JsonPropertyName("to")] public string? TokenSide { get; init; }
    /// <summary>Interval (<c>"1m"</c>, <c>"5m"</c>, <c>"15m"</c>, <c>"1h"</c>, <c>"4h"</c>, <c>"1d"</c>).</summary>
    [JsonPropertyName("i")] public string? Interval { get; init; }
    /// <summary>Open price.</summary>
    [JsonPropertyName("o")] public decimal Open { get; init; }
    /// <summary>High price.</summary>
    [JsonPropertyName("h")] public decimal High { get; init; }
    /// <summary>Low price.</summary>
    [JsonPropertyName("l")] public decimal Low { get; init; }
    /// <summary>Close price.</summary>
    [JsonPropertyName("c")] public decimal Close { get; init; }
    /// <summary>Volume.</summary>
    [JsonPropertyName("v")] public decimal? Volume { get; init; }
    /// <summary>Candle start (UTC, parsed from unix seconds).</summary>
    public DateTimeOffset CandleStart { get; init; }

    /// <summary>Raw unix seconds (G3 uses integer seconds).</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal long RawT
    {
        get => CandleStart.ToUnixTimeSeconds();
        init => CandleStart = DateTimeOffset.FromUnixTimeSeconds(value);
    }
}
