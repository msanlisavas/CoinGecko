using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Internal;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Handlers;

internal sealed class CoinGeckoPlanHandler(IOptions<CoinGeckoOptions> options) : DelegatingHandler
{
    private readonly CoinGeckoOptions _opts = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var required = request.GetRequiredPlan();
        if (required is { } min && (int)_opts.Plan < (int)min)
        {
            throw new CoinGeckoPlanException(required: min, actual: _opts.Plan);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
