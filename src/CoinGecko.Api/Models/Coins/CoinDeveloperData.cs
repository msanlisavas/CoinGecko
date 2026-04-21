using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Developer activity statistics for a coin from <c>/coins/{id}</c>.</summary>
public sealed class CoinDeveloperData
{
    /// <summary>GitHub forks.</summary>
    [JsonPropertyName("forks")] public long? Forks { get; init; }
    /// <summary>GitHub stars.</summary>
    [JsonPropertyName("stars")] public long? Stars { get; init; }
    /// <summary>GitHub subscribers / watchers.</summary>
    [JsonPropertyName("subscribers")] public long? Subscribers { get; init; }
    /// <summary>Open issues count.</summary>
    [JsonPropertyName("total_issues")] public long? TotalIssues { get; init; }
    /// <summary>Closed issues count.</summary>
    [JsonPropertyName("closed_issues")] public long? ClosedIssues { get; init; }
    /// <summary>Pull requests merged.</summary>
    [JsonPropertyName("pull_requests_merged")] public long? PullRequestsMerged { get; init; }
    /// <summary>Pull request contributors.</summary>
    [JsonPropertyName("pull_request_contributors")] public long? PullRequestContributors { get; init; }
    /// <summary>Code additions/deletions over the last 4 weeks.</summary>
    [JsonPropertyName("code_additions_deletions_4_weeks")] public CoinCodeChanges? CodeAdditionsDeletions4Weeks { get; init; }
    /// <summary>Commits over last 4 weeks.</summary>
    [JsonPropertyName("commit_count_4_weeks")] public long? CommitCount4Weeks { get; init; }
    /// <summary>Last 52 weeks of commit activity.</summary>
    [JsonPropertyName("last_52_weeks_commits")] public IReadOnlyList<long>? Last52WeeksCommits { get; init; }
}

/// <summary>Lines of code added and deleted.</summary>
public sealed class CoinCodeChanges
{
    /// <summary>Lines added.</summary>
    [JsonPropertyName("additions")] public long? Additions { get; init; }
    /// <summary>Lines deleted.</summary>
    [JsonPropertyName("deletions")] public long? Deletions { get; init; }
}
