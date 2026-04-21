using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Internal;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoRateLimitHandler(IOptions<CoinGeckoOptions> options) : DelegatingHandler
{
    private const int MaxAttempts = 4;
    private static readonly TimeSpan DefaultFallbackDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxAcceptedRetryAfter = TimeSpan.FromSeconds(60);

    private readonly CoinGeckoOptions _opts = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if ((int)response.StatusCode != 429)
            {
                return response;
            }

            var retryAfter = ReadRetryAfter(response) ?? DefaultFallbackDelay;

            if (_opts.RateLimit == RateLimitPolicy.Ignore)
            {
                return response;
            }

            if (_opts.RateLimit == RateLimitPolicy.Throw || attempt >= MaxAttempts)
            {
                var body = response.Content is null ? string.Empty : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                response.Dispose();
                throw new CoinGeckoRateLimitException(retryAfter, body, request.GetOrCreateRequestId());
            }

            response.Dispose();

            var clamped = retryAfter > MaxAcceptedRetryAfter ? MaxAcceptedRetryAfter : retryAfter;
            await Task.Delay(clamped, cancellationToken).ConfigureAwait(false);
        }
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage r)
    {
        var ra = r.Headers.RetryAfter;
        if (ra is null)
        {
            return null;
        }

        if (ra.Delta is { } delta)
        {
            return delta;
        }

        if (ra.Date is { } date)
        {
            var now = DateTimeOffset.UtcNow;
            if (date > now)
            {
                return date - now;
            }
            return TimeSpan.Zero;
        }

        return null;
    }
}
