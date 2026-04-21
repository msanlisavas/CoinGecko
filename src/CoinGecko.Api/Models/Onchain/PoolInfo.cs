using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>Metadata for a pool returned by <c>/pools/{address}/info</c>.</summary>
public sealed class PoolInfo
{
    /// <summary>Base token metadata.</summary>
    [JsonPropertyName("base_token")] public OnchainTokenMetadata? BaseToken { get; init; }
    /// <summary>Quote token metadata.</summary>
    [JsonPropertyName("quote_token")] public OnchainTokenMetadata? QuoteToken { get; init; }
}

/// <summary>Token metadata (name, symbol, coingecko coin id, etc.).</summary>
public sealed class OnchainTokenMetadata
{
    /// <summary>Resource id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Attributes (name, symbol, address, image, websites, ...).</summary>
    [JsonPropertyName("attributes")] public System.Text.Json.Nodes.JsonObject? Attributes { get; init; }
}
