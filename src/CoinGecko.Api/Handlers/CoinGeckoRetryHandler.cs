namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoRetryHandler : DelegatingHandler
{
    private const int MaxAttempts = 3;
    private static readonly Random Jitter = new();

    // Test seam:
    internal Func<int, TimeSpan> DelayProvider { get; set; } = attempt =>
    {
        // Decorrelated jitter: min = 100ms, cap = 5s
        var cap = 5000;
        var previousMs = attempt == 0 ? 100 : Math.Min(cap, (int)(Math.Pow(2, attempt - 1) * 100));
        var next = Jitter.Next(100, Math.Max(101, previousMs * 3));
        return TimeSpan.FromMilliseconds(Math.Min(cap, next));
    };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
                await Task.Delay(DelayProvider(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (attempt == MaxAttempts || !IsTransient(response.StatusCode))
            {
                return response;
            }

            response.Dispose();
            await Task.Delay(DelayProvider(attempt), cancellationToken).ConfigureAwait(false);
        }

        // Unreachable — loop either returns or throws.
        throw new InvalidOperationException("Retry loop exited unexpectedly.");
    }

    private static bool IsTransient(System.Net.HttpStatusCode code)
        => (int)code is 500 or 502 or 503 or 504;
}
