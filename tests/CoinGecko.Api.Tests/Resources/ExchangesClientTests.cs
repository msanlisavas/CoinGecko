using System.Net;
using System.Text;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class ExchangesClientTests
{
    [Fact]
    public async Task GetAsync_hits_exchanges_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/exchanges");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"binance","name":"Binance","trust_score":10,"trust_score_rank":1,"trade_volume_24h_btc":"123456.78"}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new ExchangesClient(http);

        var r = await sut.GetAsync(ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("binance");
        r[0].Name.ShouldBe("Binance");
        r[0].TrustScore.ShouldBe(10);
    }

    [Fact]
    public async Task GetListAsync_hits_exchanges_list_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/exchanges/list");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"binance","name":"Binance"},{"id":"coinbase","name":"Coinbase Exchange"}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new ExchangesClient(http);

        var r = await sut.GetListAsync(TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].Id.ShouldBe("binance");
        r[1].Name.ShouldBe("Coinbase Exchange");
    }

    [Fact]
    public async Task GetByIdAsync_hits_exchange_by_id_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/exchanges/binance");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"id":"binance","name":"Binance","country":"Cayman Islands","tickers":[]}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new ExchangesClient(http);

        var r = await sut.GetByIdAsync("binance", TestContext.Current.CancellationToken);
        r.Id.ShouldBe("binance");
        r.Country.ShouldBe("Cayman Islands");
        r.Tickers.ShouldNotBeNull();
        r.Tickers!.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetTickersAsync_hits_exchange_tickers_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/exchanges/binance/tickers");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "name":"Binance",
                        "tickers":[
                            {"base":"BTC","target":"USDT","last":42000.0,"volume":10000.0,"trust_score":"green"}
                        ]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new ExchangesClient(http);

        var r = await sut.GetTickersAsync("binance", ct: TestContext.Current.CancellationToken);
        r.Name.ShouldBe("Binance");
        r.Tickers.Count.ShouldBe(1);
        r.Tickers[0].Base.ShouldBe("BTC");
        r.Tickers[0].Target.ShouldBe("USDT");
        r.Tickers[0].TrustScore.ShouldBe("green");
    }

    [Fact]
    public async Task GetVolumeChartAsync_hits_volume_chart_and_projects_points()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/exchanges/binance/volume_chart");
            req.RequestUri!.Query.ShouldContain("days=7");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [
                        ["1713916800000","1234.5678"],
                        ["1714003200000","2345.6789"]
                    ]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new ExchangesClient(http);

        var r = await sut.GetVolumeChartAsync("binance", 7, TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        r[0].BtcVolume.ShouldBe(1234.5678m);
        r[1].BtcVolume.ShouldBe(2345.6789m);
    }

    [Fact]
    public async Task GetVolumeChartRangeAsync_hits_volume_chart_range_and_projects_points()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/exchanges/binance/volume_chart/range");
            req.RequestUri!.Query.ShouldContain("from=");
            req.RequestUri!.Query.ShouldContain("to=");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [
                        ["1713916800000","9999.0001"]
                    ]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new ExchangesClient(http);

        var from = DateTimeOffset.UnixEpoch.AddSeconds(1713916800);
        var to = from.AddDays(7);
        var r = await sut.GetVolumeChartRangeAsync("binance", from, to, TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        r[0].BtcVolume.ShouldBe(9999.0001m);
    }
}
