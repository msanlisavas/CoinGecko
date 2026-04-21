using System.Net;
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Internal;

namespace CoinGecko.Api.Handlers;

/// <summary>
/// Translates non-success HTTP responses from CoinGecko into typed <see cref="CoinGeckoException"/> subtypes.
/// Sits just inside the telemetry handler but outside the rate-limit / retry handlers, so transient 5xx
/// responses are still retried before reaching this handler; only the final response becomes an exception.
/// </summary>
internal sealed class CoinGeckoErrorHandler : DelegatingHandler
{
    private const int MaxCapturedBodyLength = 16 * 1024;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        string? body = null;
        try
        {
            if (response.Content is not null)
            {
                body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (body.Length > MaxCapturedBodyLength)
                {
                    body = body[..MaxCapturedBodyLength] + "… [truncated]";
                }
            }
        }
        catch
        {
            // Body read failures shouldn't mask the underlying status code.
        }

        var requestId = request.GetOrCreateRequestId();
        var statusCode = response.StatusCode;
        response.Dispose();

        throw (int)statusCode switch
        {
            400 => (Exception)new CoinGeckoValidationException(body, requestId),
            401 or 403 => new CoinGeckoAuthException(statusCode, body, requestId),
            404 => new CoinGeckoNotFoundException(body, requestId),
            // 429 is handled by CoinGeckoRateLimitHandler (which may still bubble past us). Fall through.
            429 => new CoinGeckoRateLimitException(retryAfter: null, body, requestId),
            >= 500 and < 600 => new CoinGeckoServerException(statusCode, body, requestId),
            _ => new CoinGeckoServerException(statusCode, body, requestId),
        };
    }
}
