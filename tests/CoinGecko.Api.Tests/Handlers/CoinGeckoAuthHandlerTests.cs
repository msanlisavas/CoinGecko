using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Tests.Infra;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoAuthHandlerTests
{
    private static HttpClient BuildClient(CoinGeckoOptions opts, out StubHandler inner)
    {
        inner = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new CoinGeckoAuthHandler(new OptionsWrapper<CoinGeckoOptions>(opts)) { InnerHandler = inner };
        return new HttpClient(handler);
    }

    [Fact]
    public async Task Demo_plan_uses_demo_header()
    {
        using var client = BuildClient(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo, ApiKey = "abc" }, out var stub);
        await client.GetAsync("https://example.com/ping", TestContext.Current.CancellationToken);
        stub.Received[0].Headers.GetValues("x-cg-demo-api-key").ShouldContain("abc");
        stub.Received[0].Headers.Contains("x-cg-pro-api-key").ShouldBeFalse();
    }

    [Fact]
    public async Task Paid_plan_uses_pro_header()
    {
        using var client = BuildClient(new CoinGeckoOptions { Plan = CoinGeckoPlan.Pro, ApiKey = "abc" }, out var stub);
        await client.GetAsync("https://example.com/ping", TestContext.Current.CancellationToken);
        stub.Received[0].Headers.GetValues("x-cg-pro-api-key").ShouldContain("abc");
        stub.Received[0].Headers.Contains("x-cg-demo-api-key").ShouldBeFalse();
    }

    [Fact]
    public async Task QueryString_mode_appends_param_and_omits_header()
    {
        using var client = BuildClient(new CoinGeckoOptions
        {
            Plan = CoinGeckoPlan.Pro,
            ApiKey = "abc",
            AuthMode = AuthenticationMode.QueryString,
        }, out var stub);

        await client.GetAsync("https://example.com/coins/markets?vs_currency=usd", TestContext.Current.CancellationToken);
        stub.Received[0].RequestUri!.Query.ShouldContain("x_cg_pro_api_key=abc");
        stub.Received[0].Headers.Contains("x-cg-pro-api-key").ShouldBeFalse();
    }

    [Fact]
    public async Task Sets_user_agent_with_version_substituted()
    {
        using var client = BuildClient(new CoinGeckoOptions { ApiKey = "abc" }, out var stub);
        await client.GetAsync("https://example.com/ping", TestContext.Current.CancellationToken);
        var ua = string.Join(' ', stub.Received[0].Headers.UserAgent.Select(p => p.ToString()));
        ua.ShouldStartWith("CoinGecko.Api/");
        ua.ShouldNotContain("{version}");
    }

    [Fact]
    public async Task Missing_api_key_does_not_add_header()
    {
        using var client = BuildClient(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo, ApiKey = null }, out var stub);
        await client.GetAsync("https://example.com/ping", TestContext.Current.CancellationToken);
        stub.Received[0].Headers.Contains("x-cg-demo-api-key").ShouldBeFalse();
    }
}
