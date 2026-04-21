using System.Diagnostics;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Observability;
using Microsoft.Extensions.Logging;

namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoTelemetryHandler(ILogger<CoinGeckoTelemetryHandler> logger) : DelegatingHandler
{
    private readonly ILogger _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var id = request.GetOrCreateRequestId();
        var sw = Stopwatch.StartNew();
        using var activity = CoinGeckoActivitySource.Instance.StartActivity(
            name: request.RequestUri?.AbsolutePath ?? "unknown",
            kind: ActivityKind.Client);
        activity?.SetTag("http.method", request.Method.Method);
        activity?.SetTag("coingecko.request_id", id);

        CoinGeckoLog.Sending(_logger, id, request.Method.Method, request.RequestUri!);

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        activity?.SetTag("http.status_code", (int)response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            CoinGeckoLog.Failed(_logger, id, (int)response.StatusCode);
            activity?.SetStatus(ActivityStatusCode.Error);
        }
        else
        {
            CoinGeckoLog.Succeeded(_logger, id, sw.Elapsed);
        }

        return response;
    }
}
