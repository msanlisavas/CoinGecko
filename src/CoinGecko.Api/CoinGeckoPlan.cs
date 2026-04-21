namespace CoinGecko.Api;

/// <summary>
/// CoinGecko subscription tiers. Ordered ascending by capability — higher-ordinal plans
/// include all endpoints and rate-limit budget of lower-ordinal plans. Use
/// <see cref="RequiresPlanAttribute"/> on sub-client methods to gate endpoints.
/// </summary>
/// <remarks>
/// The base URL split is binary: <see cref="Demo"/> routes to <c>api.coingecko.com</c>;
/// every other value routes to <c>pro-api.coingecko.com</c>. The ordinal granularity lets
/// the plan-enforcement handler short-circuit calls that require a higher tier.
/// </remarks>
public enum CoinGeckoPlan
{
    /// <summary>Free / Public tier with a Demo API key. Routes to <c>api.coingecko.com</c>.</summary>
    Demo = 0,

    /// <summary>Paid Basic tier. Routes to <c>pro-api.coingecko.com</c>.</summary>
    Basic = 1,

    /// <summary>Paid Analyst tier — unlocks WebSocket beta and premium endpoints.</summary>
    Analyst = 2,

    /// <summary>Paid Lite tier.</summary>
    Lite = 3,

    /// <summary>Paid Pro tier.</summary>
    Pro = 4,

    /// <summary>Paid Pro+ tier (higher credit / rate-limit budget than <see cref="Pro"/>).</summary>
    ProPlus = 5,

    /// <summary>Enterprise / custom contract.</summary>
    Enterprise = 6,
}
