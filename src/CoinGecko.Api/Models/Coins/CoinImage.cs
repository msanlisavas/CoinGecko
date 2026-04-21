using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Image URLs for a coin (thumb, small, large).</summary>
public sealed class CoinImage
{
    /// <summary>Thumbnail image URL.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; init; }
    /// <summary>Small image URL.</summary>
    [JsonPropertyName("small")] public string? Small { get; init; }
    /// <summary>Large image URL.</summary>
    [JsonPropertyName("large")] public string? Large { get; init; }
}
