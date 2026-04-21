using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Links associated with a coin.</summary>
public sealed class CoinLinks
{
    /// <summary>Official project home pages.</summary>
    [JsonPropertyName("homepage")] public IReadOnlyList<string>? Homepage { get; init; }
    /// <summary>Whitepaper URLs.</summary>
    [JsonPropertyName("whitepaper")] public string? Whitepaper { get; init; }
    /// <summary>Blockchain explorer URLs.</summary>
    [JsonPropertyName("blockchain_site")] public IReadOnlyList<string>? BlockchainSite { get; init; }
    /// <summary>Official forum URLs.</summary>
    [JsonPropertyName("official_forum_url")] public IReadOnlyList<string>? OfficialForumUrl { get; init; }
    /// <summary>Chat URLs.</summary>
    [JsonPropertyName("chat_url")] public IReadOnlyList<string>? ChatUrl { get; init; }
    /// <summary>Announcement channel URLs.</summary>
    [JsonPropertyName("announcement_url")] public IReadOnlyList<string>? AnnouncementUrl { get; init; }
    /// <summary>Twitter screen name.</summary>
    [JsonPropertyName("twitter_screen_name")] public string? TwitterScreenName { get; init; }
    /// <summary>Facebook username.</summary>
    [JsonPropertyName("facebook_username")] public string? FacebookUsername { get; init; }
    /// <summary>BitcoinTalk thread id.</summary>
    [JsonPropertyName("bitcointalk_thread_identifier")] public long? BitcointalkThreadIdentifier { get; init; }
    /// <summary>Telegram channel identifier.</summary>
    [JsonPropertyName("telegram_channel_identifier")] public string? TelegramChannelIdentifier { get; init; }
    /// <summary>Subreddit URL.</summary>
    [JsonPropertyName("subreddit_url")] public string? SubredditUrl { get; init; }
    /// <summary>GitHub repos.</summary>
    [JsonPropertyName("repos_url")] public CoinReposUrl? ReposUrl { get; init; }
}

/// <summary>Repository URLs for a coin.</summary>
public sealed class CoinReposUrl
{
    /// <summary>GitHub repository URLs.</summary>
    [JsonPropertyName("github")] public IReadOnlyList<string>? Github { get; init; }
    /// <summary>BitBucket repository URLs.</summary>
    [JsonPropertyName("bitbucket")] public IReadOnlyList<string>? Bitbucket { get; init; }
}
