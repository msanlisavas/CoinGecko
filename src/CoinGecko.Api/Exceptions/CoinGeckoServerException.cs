using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>5xx. Transient; retry handler exhausted or retries disabled.</summary>
public sealed class CoinGeckoServerException(HttpStatusCode statusCode, string? rawBody, Guid requestId, Exception? inner = null)
    : CoinGeckoException(
        $"CoinGecko returned {(int)statusCode} {statusCode}.",
        statusCode, rawBody, requestId, inner)
{
}
