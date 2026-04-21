using System.Net;
using System.Text;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class SimpleClientTests
{
    [Fact]
    public async Task GetPriceAsync_hits_simple_price_with_query_params_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/simple/price");
            req.RequestUri.Query.ShouldContain("ids=bitcoin%2Cethereum");
            req.RequestUri.Query.ShouldContain("vs_currencies=usd%2Ceur");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"bitcoin":{"usd":42000.5,"eur":39000.0},"ethereum":{"usd":2500.0,"eur":2300.0}}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new SimpleClient(http);

        var r = await sut.GetPriceAsync(new SimplePriceOptions
        {
            Ids = ["bitcoin", "ethereum"],
            VsCurrencies = ["usd", "eur"],
        }, TestContext.Current.CancellationToken);

        r.Count.ShouldBe(2);
        r["bitcoin"]["usd"].ShouldBe(42000.5m);
        r["ethereum"]["eur"].ShouldBe(2300.0m);
    }

    [Fact]
    public async Task GetTokenPriceAsync_hits_simple_token_price_with_platform_id_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/simple/token_price/ethereum");
            req.RequestUri.Query.ShouldContain("vs_currencies=usd");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"0xtoken":{"usd":1.5},"0xtoken2":{"usd":null}}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new SimpleClient(http);

        var r = await sut.GetTokenPriceAsync("ethereum", new SimpleTokenPriceOptions
        {
            ContractAddresses = ["0xtoken", "0xtoken2"],
            VsCurrencies = ["usd"],
        }, TestContext.Current.CancellationToken);

        r.Count.ShouldBe(2);
        r["0xtoken"]["usd"].ShouldBe(1.5m);
        r["0xtoken2"]["usd"].ShouldBeNull();
    }

    [Fact]
    public async Task GetSupportedVsCurrenciesAsync_hits_supported_vs_currencies_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/simple/supported_vs_currencies");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    ["usd","eur","gbp","btc","eth"]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new SimpleClient(http);

        var r = await sut.GetSupportedVsCurrenciesAsync(TestContext.Current.CancellationToken);
        r.Count.ShouldBe(5);
        r[0].ShouldBe("usd");
    }
}
