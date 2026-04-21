using System.Net;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoRetryHandlerTests
{
    [Fact]
    public async Task Retries_on_5xx_up_to_bounded_attempts_then_returns()
    {
        var calls = 0;
        var stub = new StubHandler((_, _) =>
        {
            calls++;
            return calls switch
            {
                1 => new HttpResponseMessage(HttpStatusCode.BadGateway),
                2 => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                _ => new HttpResponseMessage(HttpStatusCode.OK),
            };
        });

        var h = new CoinGeckoRetryHandler { InnerHandler = stub, DelayProvider = _ => TimeSpan.Zero };
        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        calls.ShouldBe(3);
    }

    [Fact]
    public async Task Does_not_retry_on_4xx()
    {
        var calls = 0;
        var stub = new StubHandler((_, _) =>
        {
            calls++;
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var h = new CoinGeckoRetryHandler { InnerHandler = stub, DelayProvider = _ => TimeSpan.Zero };
        using var client = new HttpClient(h);
        var resp = await client.GetAsync("https://example.com/x", TestContext.Current.CancellationToken);

        resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        calls.ShouldBe(1);
    }

    [Fact]
    public async Task Caller_cancellation_is_respected()
    {
        var stub = new StubHandler((_, _) => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var h = new CoinGeckoRetryHandler { InnerHandler = stub, DelayProvider = _ => TimeSpan.FromSeconds(30) };
        using var client = new HttpClient(h);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        await Should.ThrowAsync<TaskCanceledException>(
            () => client.GetAsync("https://example.com/x", cts.Token));
    }
}
