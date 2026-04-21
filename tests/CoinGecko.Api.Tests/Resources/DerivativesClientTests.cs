using System.Net;
using System.Text;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class DerivativesClientTests
{
    [Fact]
    public async Task GetTickersAsync_hits_derivatives_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/derivatives");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"market":"Binance","symbol":"BTC-PERP","contract_type":"perpetual","price":"42000","volume_24h":1000000000.0}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new DerivativesClient(http);

        var r = await sut.GetTickersAsync(TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Market.ShouldBe("Binance");
        r[0].Symbol.ShouldBe("BTC-PERP");
        r[0].Volume24h.ShouldBe(1000000000m);
    }

    [Fact]
    public async Task GetExchangesAsync_hits_derivatives_exchanges_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/derivatives/exchanges");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"binance_futures","name":"Binance Futures","trade_volume_24h_btc":"500000"}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new DerivativesClient(http);

        var r = await sut.GetExchangesAsync(ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("binance_futures");
        r[0].Name.ShouldBe("Binance Futures");
    }

    [Fact]
    public async Task GetExchangeAsync_hits_exchange_by_id_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/derivatives/exchanges/binance_futures");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"id":"binance_futures","name":"Binance Futures","open_interest_btc":50000.0,"tickers":null}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new DerivativesClient(http);

        var r = await sut.GetExchangeAsync("binance_futures", ct: TestContext.Current.CancellationToken);
        r.Id.ShouldBe("binance_futures");
        r.OpenInterestBtc.ShouldBe(50000m);
        r.Tickers.ShouldBeNull();
    }

    [Fact]
    public async Task GetExchangeListAsync_hits_derivatives_exchanges_list_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/derivatives/exchanges/list");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"binance_futures","name":"Binance Futures"},{"id":"okex_swap","name":"OKX"}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new DerivativesClient(http);

        var r = await sut.GetExchangeListAsync(TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].Id.ShouldBe("binance_futures");
        r[1].Name.ShouldBe("OKX");
    }
}
