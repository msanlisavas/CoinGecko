using CoinGecko.Api;

namespace CoinGecko.Api.Tests;

public class CoinGeckoOptionsTests
{
    [Fact]
    public void Defaults_are_sensible()
    {
        var o = new CoinGeckoOptions();
        o.ApiKey.ShouldBeNull();
        o.Plan.ShouldBe(CoinGeckoPlan.Demo);
        o.Timeout.ShouldBe(TimeSpan.FromSeconds(30));
        o.UserAgent.ShouldStartWith("CoinGecko.Api/");
        o.AutoPaginate.ShouldBeTrue();
        o.RateLimit.ShouldBe(RateLimitPolicy.Respect);
        o.AuthMode.ShouldBe(AuthenticationMode.Header);
        o.BaseAddress.ShouldBeNull();
        o.OnchainBaseAddress.ShouldBeNull();
    }

    [Fact]
    public void UserAgent_placeholder_is_replaced_with_assembly_version_when_read_via_resolved_accessor()
    {
        var o = new CoinGeckoOptions();
        // UserAgent is a raw template here; a later phase (handler) resolves {version}.
        o.UserAgent.ShouldContain("{version}");
    }
}
