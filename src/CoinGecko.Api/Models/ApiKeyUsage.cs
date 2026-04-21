using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Current API key usage, from <c>GET /key</c>.</summary>
public sealed class ApiKeyUsage
{
    /// <summary>Plan name (e.g. <c>"Analyst"</c>).</summary>
    [JsonPropertyName("plan")] public string? Plan { get; init; }

    /// <summary>Rate limit in requests per minute.</summary>
    [JsonPropertyName("rate_limit_request_per_minute")] public int RateLimitRequestPerMinute { get; init; }

    /// <summary>Monthly credit quota.</summary>
    [JsonPropertyName("monthly_call_credit")] public long MonthlyCallCredit { get; init; }

    /// <summary>Calls consumed this month.</summary>
    [JsonPropertyName("current_total_monthly_calls")] public long CurrentTotalMonthlyCalls { get; init; }

    /// <summary>Remaining calls this month.</summary>
    [JsonPropertyName("current_remaining_monthly_calls")] public long CurrentRemainingMonthlyCalls { get; init; }
}
