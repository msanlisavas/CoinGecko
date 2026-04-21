using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>401 / 403. Typically a missing, wrong-type, or revoked API key.</summary>
public sealed class CoinGeckoAuthException(HttpStatusCode statusCode, string? rawBody, Guid requestId)
    : CoinGeckoException(
        $"CoinGecko rejected the credentials ({(int)statusCode} {statusCode}). Verify ApiKey and Plan.",
        statusCode, rawBody, requestId)
{
}
