using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoAuthHandler(IOptions<CoinGeckoOptions> options) : DelegatingHandler
{
    private static readonly string LibVersion =
        typeof(CoinGeckoAuthHandler).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(CoinGeckoAuthHandler).Assembly.GetName().Version?.ToString()
        ?? "0.0.0";

    private readonly CoinGeckoOptions _opts = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AttachUserAgent(request);
        AttachApiKey(request);
        AttachAccept(request);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private void AttachUserAgent(HttpRequestMessage req)
    {
        req.Headers.UserAgent.Clear();
        var resolved = _opts.UserAgent.Replace("{version}", LibVersion);
        if (ProductInfoHeaderValue.TryParse(resolved, out var parsed))
        {
            req.Headers.UserAgent.Add(parsed!);
        }
        else
        {
            req.Headers.UserAgent.ParseAdd(resolved);
        }
    }

    private void AttachApiKey(HttpRequestMessage req)
    {
        if (string.IsNullOrEmpty(_opts.ApiKey))
        {
            return;
        }

        var headerName = _opts.Plan == CoinGeckoPlan.Demo ? "x-cg-demo-api-key" : "x-cg-pro-api-key";
        var paramName  = _opts.Plan == CoinGeckoPlan.Demo ? "x_cg_demo_api_key" : "x_cg_pro_api_key";

        if (_opts.AuthMode == AuthenticationMode.Header)
        {
            req.Headers.Remove(headerName);
            req.Headers.Add(headerName, _opts.ApiKey);
        }
        else
        {
            var uri = req.RequestUri ?? throw new InvalidOperationException("Request has no URI.");
            var sep = string.IsNullOrEmpty(uri.Query) ? '?' : '&';
            req.RequestUri = new Uri($"{uri}{sep}{paramName}={Uri.EscapeDataString(_opts.ApiKey)}", UriKind.Absolute);
        }
    }

    private static void AttachAccept(HttpRequestMessage req)
    {
        if (req.Headers.Accept.Count == 0)
        {
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
