using System.Net;

namespace CoinGecko.Api.Exceptions;

/// <summary>Base type for all exceptions thrown by <c>CoinGecko.Api</c>.</summary>
public abstract class CoinGeckoException : Exception
{
    /// <summary>HTTP status of the CoinGecko response, or null if the error pre-dates the HTTP call.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>Raw response body (truncated to 16 KiB).</summary>
    public string? RawBody { get; }

    /// <summary>Correlation id shared with <c>ActivitySource</c> and <c>ILogger</c>.</summary>
    public Guid RequestId { get; }

    /// <summary>Initializes a new instance of <see cref="CoinGeckoException"/>.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">HTTP status code, or <c>null</c> if not applicable.</param>
    /// <param name="rawBody">Raw response body text.</param>
    /// <param name="requestId">Correlation identifier for tracing.</param>
    /// <param name="inner">Optional inner exception.</param>
    protected CoinGeckoException(
        string message,
        HttpStatusCode? statusCode,
        string? rawBody,
        Guid requestId,
        Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        RawBody = Truncate(rawBody, 16 * 1024);
        RequestId = requestId;
    }

    private static string? Truncate(string? s, int max)
        => s is null || s.Length <= max ? s : s[..max] + "… [truncated]";
}
