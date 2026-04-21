using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class PingClientTests
{
    [Fact]
    public async Task PingAsync_hits_ping_path_and_returns_message()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/ping");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"gecko_says":"(V3) To the Moon!"}""", Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new PingClient(http);

        var resp = await sut.PingAsync(TestContext.Current.CancellationToken);
        resp.GeckoSays.ShouldBe("(V3) To the Moon!");
    }
}
