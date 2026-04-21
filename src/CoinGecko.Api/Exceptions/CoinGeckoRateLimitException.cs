using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>Thrown when CoinGecko responds 429 and <see cref="RateLimitPolicy.Throw"/> is in effect, or when retries are exhausted under <see cref="RateLimitPolicy.Respect"/>.</summary>
public sealed class CoinGeckoRateLimitException(TimeSpan? retryAfter, string? rawBody, Guid requestId)
    : CoinGeckoException(
        $"CoinGecko rate limit hit (429). Retry-After = {retryAfter?.ToString() ?? "unspecified"}.",
        HttpStatusCode.TooManyRequests,
        rawBody,
        requestId)
{
    /// <summary>The server-suggested retry delay, or <c>null</c> if the header was absent.</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
