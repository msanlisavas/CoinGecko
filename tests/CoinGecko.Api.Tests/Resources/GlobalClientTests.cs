using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class GlobalClientTests
{
    [Fact]
    public async Task GetAsync_hits_global_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/global");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"data":{"active_cryptocurrencies":10000,"markets":800,"market_cap_change_percentage_24h_usd":1.5,"updated_at":1713916800}}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new GlobalClient(http);

        var r = await sut.GetAsync(TestContext.Current.CancellationToken);
        r.ActiveCryptocurrencies.ShouldBe(10000);
        r.Markets.ShouldBe(800);
        r.MarketCapChangePercentage24hUsd.ShouldBe(1.5m);
    }

    [Fact]
    public async Task GetDefiAsync_hits_defi_endpoint_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/global/decentralized_finance_defi");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"data":{"defi_market_cap":"120000000000","defi_dominance":"4.5","top_coin_name":"Uniswap","top_coin_defi_dominance":12.3}}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new GlobalClient(http);

        var r = await sut.GetDefiAsync(TestContext.Current.CancellationToken);
        r.DefiMarketCap.ShouldBe("120000000000");
        r.TopCoinName.ShouldBe("Uniswap");
        r.TopCoinDefiDominance.ShouldBe(12.3m);
    }

    [Fact]
    public async Task GetMarketCapChartAsync_merges_arrays_into_points()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/global/market_cap_chart");
            req.RequestUri.Query.ShouldContain("days=7");
            req.RequestUri.Query.ShouldContain("vs_currency=usd");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"market_cap_chart":{"market_cap":[[1713916800000,2500000000000],[1714003200000,2600000000000]],"volume":[[1713916800000,80000000000],[1714003200000,85000000000]]}}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new GlobalClient(http);

        var r = await sut.GetMarketCapChartAsync(7, "usd", TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].MarketCap.ShouldBe(2500000000000m);
        r[0].Volume24h.ShouldBe(80000000000m);
        r[1].MarketCap.ShouldBe(2600000000000m);
        r[0].Timestamp.ToUnixTimeMilliseconds().ShouldBe(1713916800000L);
    }
}
