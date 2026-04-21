using System.Text.Json.Serialization;

namespace CoinGecko.Api.WebSockets.Ticks;

/// <summary>G1 channel tick — GeckoTerminal onchain token price.</summary>
public sealed class OnchainTokenPriceTick
{
    /// <summary>Channel code (always <c>"G1"</c>).</summary>
    [JsonPropertyName("ch")] public string? ChannelCode { get; init; }
    /// <summary>Network id.</summary>
    [JsonPropertyName("n")] public string? NetworkId { get; init; }
    /// <summary>Token contract address.</summary>
    [JsonPropertyName("ta")] public string? TokenAddress { get; init; }
    /// <summary>Price in USD.</summary>
    [JsonPropertyName("pu")] public decimal? PriceUsd { get; init; }
    /// <summary>Price in native currency.</summary>
    [JsonPropertyName("pn")] public decimal? PriceNative { get; init; }
    /// <summary>Fully-diluted valuation USD.</summary>
    [JsonPropertyName("fdv")] public decimal? FdvUsd { get; init; }
    /// <summary>Total reserve USD across tracked pools.</summary>
    [JsonPropertyName("tr")] public decimal? TotalReserveUsd { get; init; }
    /// <summary>UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Raw unix-seconds float.</summary>
    [JsonPropertyName("t")]
    [JsonInclude]
    internal double RawT
    {
        get => Timestamp.ToUnixTimeMilliseconds() / 1000.0;
        init => Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)(value * 1000));
    }
}
