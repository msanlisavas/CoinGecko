using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class SearchClientTests
{
    [Fact]
    public async Task SearchAsync_hits_search_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/search");
            req.RequestUri.Query.ShouldContain("query=bitcoin");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"coins":[{"id":"bitcoin","name":"Bitcoin","symbol":"BTC","market_cap_rank":1}],"exchanges":[],"categories":[],"nfts":[]}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new SearchClient(http);

        var r = await sut.SearchAsync("bitcoin", TestContext.Current.CancellationToken);
        r.Coins.Count.ShouldBe(1);
        r.Coins[0].Id.ShouldBe("bitcoin");
    }
}
