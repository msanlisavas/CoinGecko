using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>OHLCV envelope returned by <c>/pools/{address}/ohlcv/{timeframe}</c>.</summary>
public sealed class OnchainOhlcvResource
{
    /// <summary>Resource id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>OHLCV attributes.</summary>
    [JsonPropertyName("attributes")] public OnchainOhlcvAttributes? Attributes { get; init; }
    /// <summary>Cross-reference meta (base/quote token info).</summary>
    [JsonPropertyName("meta")] public System.Text.Json.Nodes.JsonObject? Meta { get; init; }
}

/// <summary>Raw OHLCV rows.</summary>
public sealed class OnchainOhlcvAttributes
{
    /// <summary>Array of rows <c>[ts_seconds, o, h, l, c, volume_usd]</c>.</summary>
    [JsonPropertyName("ohlcv_list")] public decimal[][]? OhlcvList { get; init; }
}

/// <summary>Strongly-typed OHLCV row (projected from the raw array).</summary>
public sealed record OnchainOhlcv(
    DateTimeOffset Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal VolumeUsd);
