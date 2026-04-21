using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>Token metadata from <c>/tokens/{address}/info</c>.</summary>
public sealed class OnchainTokenInfo
{
    /// <summary>Resource id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Extended metadata attributes.</summary>
    [JsonPropertyName("attributes")] public OnchainTokenInfoAttributes? Attributes { get; init; }
}

/// <summary>Extended token metadata (description, websites, socials).</summary>
public sealed class OnchainTokenInfoAttributes
{
    /// <summary>Token address.</summary>
    [JsonPropertyName("address")] public string? Address { get; init; }
    /// <summary>Name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Description.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    /// <summary>Image URL.</summary>
    [JsonPropertyName("image_url")] public string? ImageUrl { get; init; }
    /// <summary>Websites array.</summary>
    [JsonPropertyName("websites")] public IReadOnlyList<string>? Websites { get; init; }
    /// <summary>Discord URL.</summary>
    [JsonPropertyName("discord_url")] public string? DiscordUrl { get; init; }
    /// <summary>Telegram handle.</summary>
    [JsonPropertyName("telegram_handle")] public string? TelegramHandle { get; init; }
    /// <summary>Twitter handle.</summary>
    [JsonPropertyName("twitter_handle")] public string? TwitterHandle { get; init; }
    /// <summary>CoinGecko coin id (if mapped).</summary>
    [JsonPropertyName("coingecko_coin_id")] public string? CoingeckoCoinId { get; init; }
    /// <summary>Categories the token belongs to.</summary>
    [JsonPropertyName("categories")] public IReadOnlyList<string>? Categories { get; init; }
    /// <summary>Tags.</summary>
    [JsonPropertyName("gt_categories_id")] public IReadOnlyList<string>? GtCategoriesId { get; init; }
}
