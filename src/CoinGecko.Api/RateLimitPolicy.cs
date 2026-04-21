namespace CoinGecko.Api;

/// <summary>
/// Behavior when CoinGecko responds with <c>429 Too Many Requests</c>.
/// </summary>
public enum RateLimitPolicy
{
    /// <summary>Honor <c>Retry-After</c> and retry automatically (default).</summary>
    Respect = 0,

    /// <summary>Surface as <c>CoinGeckoRateLimitException</c> without retrying.</summary>
    Throw = 1,

    /// <summary>Pass the raw <c>HttpResponseMessage</c> through without special handling.</summary>
    Ignore = 2,
}
