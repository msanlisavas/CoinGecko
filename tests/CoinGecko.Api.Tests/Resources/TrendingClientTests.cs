using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class TrendingClientTests
{
    [Fact]
    public async Task GetAsync_hits_search_trending_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/search/trending");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"coins":[{"item":{"id":"bitcoin","name":"Bitcoin","symbol":"BTC","market_cap_rank":1}}],"nfts":[],"categories":[]}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new TrendingClient(http);

        var r = await sut.GetAsync(ct: TestContext.Current.CancellationToken);
        r.Coins.Count.ShouldBe(1);
        r.Coins[0].Item!.Id.ShouldBe("bitcoin");
    }
}
