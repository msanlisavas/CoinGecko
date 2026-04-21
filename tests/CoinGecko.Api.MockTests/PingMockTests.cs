using CoinGecko.Api.MockTests.Infra;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CoinGecko.Api.MockTests;

public class PingMockTests : IClassFixture<CoinGeckoMockFixture>
{
    private readonly CoinGeckoMockFixture _fx;

    public PingMockTests(CoinGeckoMockFixture fx) => _fx = fx;

    [Fact]
    public async Task Ping_happy_path()
    {
        _fx.Server
            .Given(Request.Create().WithPath("/api/v3/ping").UsingGet()
                .WithHeader("x-cg-demo-api-key", "test-demo-key"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"gecko_says":"(V3) To the Moon!"}"""));

        var r = await _fx.Client.Ping.PingAsync(TestContext.Current.CancellationToken);
        r.GeckoSays.ShouldBe("(V3) To the Moon!");
    }
}
