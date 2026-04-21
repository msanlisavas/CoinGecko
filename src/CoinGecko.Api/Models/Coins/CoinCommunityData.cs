using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Community statistics for a coin from <c>/coins/{id}</c>.</summary>
public sealed class CoinCommunityData
{
    /// <summary>Facebook likes.</summary>
    [JsonPropertyName("facebook_likes")] public long? FacebookLikes { get; init; }
    /// <summary>Twitter followers.</summary>
    [JsonPropertyName("twitter_followers")] public long? TwitterFollowers { get; init; }
    /// <summary>Reddit average posts per 48h.</summary>
    [JsonPropertyName("reddit_average_posts_48h")] public decimal? RedditAveragePosts48h { get; init; }
    /// <summary>Reddit average comments per 48h.</summary>
    [JsonPropertyName("reddit_average_comments_48h")] public decimal? RedditAverageComments48h { get; init; }
    /// <summary>Reddit subscribers.</summary>
    [JsonPropertyName("reddit_subscribers")] public long? RedditSubscribers { get; init; }
    /// <summary>Reddit accounts active in the past 48h.</summary>
    [JsonPropertyName("reddit_accounts_active_48h")] public long? RedditAccountsActive48h { get; init; }
    /// <summary>Telegram channel user count.</summary>
    [JsonPropertyName("telegram_channel_user_count")] public long? TelegramChannelUserCount { get; init; }
}
