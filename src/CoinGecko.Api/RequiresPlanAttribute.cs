namespace CoinGecko.Api;

/// <summary>
/// Marks a sub-client interface, class, or method as requiring a specific minimum
/// <see cref="CoinGeckoPlan"/> tier. Enforced at the handler-pipeline level (see
/// <c>CoinGeckoPlanHandler</c>) — calls that violate the attribute throw
/// <c>CoinGeckoPlanException</c> before the HTTP request is issued.
/// </summary>
/// <remarks>
/// Ordinal comparison is used: <c>[RequiresPlan(CoinGeckoPlan.Analyst)]</c> passes for
/// <see cref="CoinGeckoPlan.Analyst"/> and every higher tier.
/// </remarks>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface,
    AllowMultiple = false,
    Inherited = false)]
public sealed class RequiresPlanAttribute(CoinGeckoPlan plan) : Attribute
{
    /// <summary>The minimum plan tier that can invoke the decorated member.</summary>
    public CoinGeckoPlan Plan { get; } = plan;
}
