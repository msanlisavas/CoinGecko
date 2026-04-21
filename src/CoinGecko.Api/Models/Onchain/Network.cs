using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>A blockchain network supported by CoinGecko's onchain / GeckoTerminal surface.</summary>
public sealed class Network
{
    /// <summary>Network id (e.g. <c>"eth"</c>, <c>"bsc"</c>).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type (always <c>"network"</c>).</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Network attributes.</summary>
    [JsonPropertyName("attributes")] public NetworkAttributes? Attributes { get; init; }
}

/// <summary>Network display data.</summary>
public sealed class NetworkAttributes
{
    /// <summary>Display name (e.g. <c>"Ethereum"</c>).</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>CoinGecko asset-platform id for cross-referencing with /asset_platforms.</summary>
    [JsonPropertyName("coingecko_asset_platform_id")] public string? CoingeckoAssetPlatformId { get; init; }
}
