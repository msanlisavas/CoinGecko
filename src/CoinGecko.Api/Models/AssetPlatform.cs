using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>A blockchain / asset platform supported by CoinGecko.</summary>
public sealed class AssetPlatform
{
    /// <summary>Platform id (e.g. <c>"ethereum"</c>).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }

    /// <summary>Chain id, if applicable (e.g. 1 for Ethereum mainnet).</summary>
    [JsonPropertyName("chain_identifier")] public long? ChainIdentifier { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Short name / ticker.</summary>
    [JsonPropertyName("shortname")] public string? Shortname { get; init; }

    /// <summary>Native coin id (e.g. <c>"ethereum"</c>).</summary>
    [JsonPropertyName("native_coin_id")] public string? NativeCoinId { get; init; }

    /// <summary>Icon URL, if available.</summary>
    [JsonPropertyName("image")] public AssetPlatformImage? Image { get; init; }
}

/// <summary>Icon variants for an asset platform.</summary>
public sealed class AssetPlatformImage
{
    /// <summary>Small icon URL.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; init; }

    /// <summary>Medium icon URL.</summary>
    [JsonPropertyName("small")] public string? Small { get; init; }

    /// <summary>Large icon URL.</summary>
    [JsonPropertyName("large")] public string? Large { get; init; }
}
