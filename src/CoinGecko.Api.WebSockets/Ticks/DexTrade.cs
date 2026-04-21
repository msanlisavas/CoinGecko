using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>G2 channel tick — DEX pool trade (swap).</summary>
public sealed class DexTrade
{
    /// <summary>Channel code (always <c>"G2"</c>).</summary>
    [JsonPropertyName("ch")] public string? ChannelCode { get; init; }
    /// <summary>Network id.</summary>
    [JsonPropertyName("n")] public string? NetworkId { get; init; }
    /// <summary>Pool contract address.</summary>
    [JsonPropertyName("pa")] public string? PoolAddress { get; init; }
    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("tx")] public string? TxHash { get; init; }
    /// <summary>Trade type — <c>"b"</c> (buy base) or <c>"s"</c> (sell base).</summary>
    [JsonPropertyName("ty")] public string? TradeType { get; init; }
    /// <summary>Base token amount.</summary>
    [JsonPropertyName("to")] public decimal? BaseTokenAmount { get; init; }
    /// <summary>Quote token amount.</summary>
    [JsonPropertyName("toq")] public decimal? QuoteTokenAmount { get; init; }
    /// <summary>Trade volume in USD.</summary>
    [JsonPropertyName("vo")] public decimal? VolumeUsd { get; init; }
    /// <summary>Base price in native currency.</summary>
    [JsonPropertyName("pc")] public decimal? PriceNative { get; init; }
    /// <summary>Base price in USD.</summary>
    [JsonPropertyName("pu")] public decimal? PriceUsd { get; init; }
    /// <summary>UTC timestamp (G2 uses ms-since-epoch).</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Raw unix milliseconds.</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal long RawT
    {
        get => Timestamp.ToUnixTimeMilliseconds();
        init => Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(value);
    }
}
