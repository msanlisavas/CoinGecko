using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Tests.Infra;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoRateLimitHandlerTests
{
    private static HttpResponseMessage RateLimited(int? retryAfterSeconds)
    {
        var r = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (retryAfterSeconds is not null)
        {
            r.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(retryAfterSeconds.Value));
        }
        return r;
    }

    [Fact]
    public async Task Respect_policy_retries_after_header_value_then_succeeds()
    {
        var calls = 0;
        var stub = new StubHandler((req, _) =>
        {
            calls++;
            return calls switch
            {
                1 => RateLimited(0), // immediate retry
                _ => new HttpResponseMessage(HttpStatusCode.OK),
            };
        });

        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Respect }))
        { InnerHandler = stub };

        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x", TestContext.Current.CancellationToken);
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task Throw_policy_surfaces_exception_with_retry_after()
    {
        var stub = new StubHandler(RateLimited(7));
        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Throw }))
        { InnerHandler = stub };

        using var client = new HttpClient(h);
        var ex = await Should.ThrowAsync<CoinGeckoRateLimitException>(() => client.GetAsync("https://example.com/x", TestContext.Current.CancellationToken));
        ex.RetryAfter.ShouldBe(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public async Task Ignore_policy_passes_429_through()
    {
        var stub = new StubHandler(RateLimited(1));
        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Ignore }))
        { InnerHandler = stub };

        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x", TestContext.Current.CancellationToken);
        resp.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Exhausts_retries_then_throws_RateLimitException()
    {
        var stub = new StubHandler(RateLimited(0));
        var h = new CoinGeckoRateLimitHandler(
            new OptionsWrapper<CoinGeckoOptions>(new CoinGeckoOptions { RateLimit = RateLimitPolicy.Respect }))
        { InnerHandler = stub };

        using var client = new HttpClient(h);
        await Should.ThrowAsync<CoinGeckoRateLimitException>(() => client.GetAsync("https://example.com/x", TestContext.Current.CancellationToken));
    }
}
