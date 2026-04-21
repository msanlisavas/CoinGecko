using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class CategoriesClientTests
{
    [Fact]
    public async Task GetListAsync_hits_categories_list_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/categories/list");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"category_id":"decentralized-finance-defi","name":"Decentralized Finance (DeFi)"}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CategoriesClient(http);

        var r = await sut.GetListAsync(TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].CategoryId.ShouldBe("decentralized-finance-defi");
    }

    [Fact]
    public async Task GetAsync_hits_categories_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/categories");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"defi","name":"DeFi","market_cap":100000000.0,"market_cap_change_24h":1.5}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CategoriesClient(http);

        var r = await sut.GetAsync(ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("defi");
    }
}
