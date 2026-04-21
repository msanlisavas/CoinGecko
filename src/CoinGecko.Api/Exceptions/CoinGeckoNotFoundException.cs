using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>404. Usually an unknown coin id, asset-platform id, or contract address.</summary>
public sealed class CoinGeckoNotFoundException(string? rawBody, Guid requestId)
    : CoinGeckoException(
        "CoinGecko returned 404 Not Found.",
        HttpStatusCode.NotFound, rawBody, requestId)
{
}
