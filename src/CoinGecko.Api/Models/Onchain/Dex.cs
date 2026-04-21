using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>A DEX on a supported onchain network.</summary>
public sealed class Dex
{
    /// <summary>DEX id (e.g. <c>"uniswap_v3"</c>).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type (always <c>"dex"</c>).</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>DEX attributes.</summary>
    [JsonPropertyName("attributes")] public DexAttributes? Attributes { get; init; }
}

/// <summary>DEX display data.</summary>
public sealed class DexAttributes
{
    /// <summary>DEX display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
}
