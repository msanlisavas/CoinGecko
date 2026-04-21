using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class PolicyEnumTests
{
    [Fact]
    public void RateLimitPolicy_has_three_values_with_respect_as_default()
    {
        Enum.GetValues<RateLimitPolicy>().ShouldBe(
            new[] { RateLimitPolicy.Respect, RateLimitPolicy.Throw, RateLimitPolicy.Ignore },
            ignoreOrder: true);
        default(RateLimitPolicy).ShouldBe(RateLimitPolicy.Respect);
    }

    [Fact]
    public void AuthenticationMode_has_header_default_and_querystring_alt()
    {
        Enum.GetValues<AuthenticationMode>().ShouldBe(
            new[] { AuthenticationMode.Header, AuthenticationMode.QueryString },
            ignoreOrder: true);
        default(AuthenticationMode).ShouldBe(AuthenticationMode.Header);
    }
}
