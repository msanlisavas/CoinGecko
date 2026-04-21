using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>400. Invalid query parameters or body.</summary>
public sealed class CoinGeckoValidationException(string? rawBody, Guid requestId)
    : CoinGeckoException(
        $"CoinGecko returned 400 Bad Request. Body: {rawBody}",
        HttpStatusCode.BadRequest, rawBody, requestId)
{
}
