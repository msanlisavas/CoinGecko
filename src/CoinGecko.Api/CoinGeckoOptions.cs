namespace CoinGecko.Api;

/// <summary>
/// Configuration for <c>ICoinGeckoClient</c>. Bind from configuration or set directly
/// via <c>AddCoinGeckoApi(opts =&gt; ...)</c>.
/// </summary>
public sealed class CoinGeckoOptions
{
    /// <summary>CoinGecko API key. Required for every tier (Demo keys are free but required since 2024).</summary>
    public string? ApiKey { get; set; }

    /// <summary>Subscription tier. Drives base URL selection (Demo → <c>api.coingecko.com</c>; anything else → <c>pro-api.coingecko.com</c>) and endpoint gating.</summary>
    public CoinGeckoPlan Plan { get; set; } = CoinGeckoPlan.Demo;

    /// <summary>Override for the primary base URL. Leave null for the plan-default host.</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Override for the onchain (GeckoTerminal) base URL. Leave null to derive from <see cref="BaseAddress"/>.</summary>
    public Uri? OnchainBaseAddress { get; set; }

    /// <summary>User-Agent header. The token <c>{version}</c> is replaced with the assembly informational version at handler attach time.</summary>
    public string UserAgent { get; set; } = "CoinGecko.Api/{version} (+https://github.com/msanlisavas/CoinGecko)";

    /// <summary>Per-request timeout. Applied via a linked <see cref="CancellationTokenSource"/>; does not touch <see cref="HttpClient.Timeout"/>.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Enable <see cref="IAsyncEnumerable{T}"/> auto-pagination for <c>EnumerateXxxAsync</c> methods.</summary>
    public bool AutoPaginate { get; set; } = true;

    /// <summary>Behavior on HTTP 429 responses.</summary>
    public RateLimitPolicy RateLimit { get; set; } = RateLimitPolicy.Respect;

    /// <summary>Whether to transmit the API key via header (default) or query string.</summary>
    public AuthenticationMode AuthMode { get; set; } = AuthenticationMode.Header;
}
