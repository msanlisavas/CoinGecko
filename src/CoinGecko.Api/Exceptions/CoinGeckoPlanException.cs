namespace CoinGecko.Api.Exceptions;

/// <summary>Thrown before issuing a request when the configured <see cref="CoinGeckoOptions.Plan"/> is below the endpoint's <see cref="RequiresPlanAttribute"/>.</summary>
public sealed class CoinGeckoPlanException(CoinGeckoPlan required, CoinGeckoPlan actual)
    : CoinGeckoException(
        $"This endpoint requires plan {required} or higher; configured plan is {actual}. Upgrade at https://www.coingecko.com/en/api/pricing.",
        statusCode: null,
        rawBody: null,
        requestId: Guid.Empty)
{
    /// <summary>The minimum plan required by the endpoint.</summary>
    public CoinGeckoPlan RequiredPlan { get; } = required;

    /// <summary>The plan that is currently configured.</summary>
    public CoinGeckoPlan ActualPlan { get; } = actual;
}
