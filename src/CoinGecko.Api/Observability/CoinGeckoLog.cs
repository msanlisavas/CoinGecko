using Microsoft.Extensions.Logging;

namespace CoinGecko.Api.Observability;

internal static partial class CoinGeckoLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug,
        Message = "CoinGecko request {RequestId}: {Method} {Url}")]
    public static partial void Sending(ILogger logger, Guid requestId, string method, Uri url);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "CoinGecko rate-limited {RequestId}: retry after {RetryAfter}")]
    public static partial void RateLimited(ILogger logger, Guid requestId, TimeSpan retryAfter);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug,
        Message = "CoinGecko retry {RequestId}: attempt {Attempt}")]
    public static partial void Retrying(ILogger logger, Guid requestId, int attempt);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning,
        Message = "CoinGecko request {RequestId} failed: {StatusCode}")]
    public static partial void Failed(ILogger logger, Guid requestId, int statusCode);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug,
        Message = "CoinGecko request {RequestId} succeeded in {Elapsed}")]
    public static partial void Succeeded(ILogger logger, Guid requestId, TimeSpan elapsed);
}
