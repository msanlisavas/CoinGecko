using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.News;

/// <summary>An article returned from <c>GET /news</c>.</summary>
public sealed class NewsArticle
{
    /// <summary>Article title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
    /// <summary>Article URL.</summary>
    [JsonPropertyName("url")] public string? Url { get; init; }
    /// <summary>Image URL.</summary>
    [JsonPropertyName("image")] public string? Image { get; init; }
    /// <summary>Author name.</summary>
    [JsonPropertyName("author")] public string? Author { get; init; }
    /// <summary>Posted at (ISO-8601).</summary>
    [JsonPropertyName("posted_at")] public string? PostedAt { get; init; }
    /// <summary>Item type — typically <c>"news"</c> or <c>"guide"</c>.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Source publisher identifier.</summary>
    [JsonPropertyName("source_name")] public string? SourceName { get; init; }
    /// <summary>CoinGecko coin ids referenced by the article.</summary>
    [JsonPropertyName("related_coin_ids")] public IReadOnlyList<string>? RelatedCoinIds { get; init; }
}
