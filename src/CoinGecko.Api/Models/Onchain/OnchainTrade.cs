using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>A trade observed on a pool.</summary>
public sealed class OnchainTrade
{
    /// <summary>Trade id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type (always <c>"trade"</c>).</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Trade attributes.</summary>
    [JsonPropertyName("attributes")] public OnchainTradeAttributes? Attributes { get; init; }
}

/// <summary>Trade attributes.</summary>
public sealed class OnchainTradeAttributes
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("block_number")] public long? BlockNumber { get; init; }
    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("tx_hash")] public string? TxHash { get; init; }
    /// <summary>Originator wallet.</summary>
    [JsonPropertyName("tx_from_address")] public string? TxFromAddress { get; init; }
    /// <summary>Base-token-to-quote direction (<c>"buy"</c> / <c>"sell"</c>).</summary>
    [JsonPropertyName("kind")] public string? Kind { get; init; }
    /// <summary>Volume in USD.</summary>
    [JsonPropertyName("volume_in_usd")] public decimal? VolumeInUsd { get; init; }
    /// <summary>Base token amount.</summary>
    [JsonPropertyName("from_token_amount")] public decimal? FromTokenAmount { get; init; }
    /// <summary>Quote token amount.</summary>
    [JsonPropertyName("to_token_amount")] public decimal? ToTokenAmount { get; init; }
    /// <summary>Price of base in USD.</summary>
    [JsonPropertyName("price_from_in_usd")] public decimal? PriceFromInUsd { get; init; }
    /// <summary>Price of quote in USD.</summary>
    [JsonPropertyName("price_to_in_usd")] public decimal? PriceToInUsd { get; init; }
    /// <summary>Block timestamp (ISO-8601).</summary>
    [JsonPropertyName("block_timestamp")] public string? BlockTimestamp { get; init; }
}
