using System.Net;
using System.Text;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class CoinsClientTests
{
    [Fact]
    public async Task GetListAsync_hits_coins_list_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/list");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"bitcoin","symbol":"btc","name":"Bitcoin"},{"id":"ethereum","symbol":"eth","name":"Ethereum"}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetListAsync(ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].Id.ShouldBe("bitcoin");
        r[1].Symbol.ShouldBe("eth");
    }

    [Fact]
    public async Task GetListAsync_include_platform_adds_query_param()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.RequestUri!.Query.ShouldContain("include_platform=true");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"bitcoin","symbol":"btc","name":"Bitcoin","platforms":{"ethereum":"0xabc"}}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetListAsync(includePlatform: true, ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Platforms.ShouldNotBeNull();
        r[0].Platforms!["ethereum"].ShouldBe("0xabc");
    }

    [Fact]
    public async Task GetMarketsAsync_hits_coins_markets_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/markets");
            req.RequestUri!.Query.ShouldContain("vs_currency=usd");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [
                        {
                            "id":"bitcoin",
                            "symbol":"btc",
                            "name":"Bitcoin",
                            "current_price":42000.5,
                            "market_cap":800000000000,
                            "market_cap_rank":1
                        }
                    ]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetMarketsAsync("usd", ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("bitcoin");
        r[0].CurrentPrice.ShouldBe(42000.5m);
        r[0].MarketCapRank.ShouldBe(1);
    }

    [Fact]
    public async Task GetAsync_hits_coin_by_id_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "id":"bitcoin",
                        "symbol":"btc",
                        "name":"Bitcoin",
                        "market_cap_rank":1,
                        "categories":["Cryptocurrency"]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetAsync("bitcoin", ct: TestContext.Current.CancellationToken);
        r.Id.ShouldBe("bitcoin");
        r.MarketCapRank.ShouldBe(1);
        r.Categories.ShouldNotBeNull();
        r.Categories!.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetTickersAsync_hits_coin_tickers_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/tickers");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "name":"Bitcoin",
                        "tickers":[
                            {"base":"BTC","target":"USDT","last":42000.0,"volume":1000.0,"trust_score":"green"}
                        ]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetTickersAsync("bitcoin", ct: TestContext.Current.CancellationToken);
        r.Name.ShouldBe("Bitcoin");
        r.Tickers.Count.ShouldBe(1);
        r.Tickers[0].Base.ShouldBe("BTC");
        r.Tickers[0].TrustScore.ShouldBe("green");
    }

    [Fact]
    public async Task GetHistoryAsync_hits_coin_history_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/history");
            req.RequestUri!.Query.ShouldContain("date=30-12-2023");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "id":"bitcoin",
                        "symbol":"btc",
                        "name":"Bitcoin",
                        "market_data":{
                            "current_price":{"usd":42500.0},
                            "market_cap":{"usd":830000000000}
                        }
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetHistoryAsync("bitcoin", new DateOnly(2023, 12, 30), ct: TestContext.Current.CancellationToken);
        r.Id.ShouldBe("bitcoin");
        r.MarketData.ShouldNotBeNull();
        r.MarketData!.CurrentPrice!["usd"].ShouldBe(42500.0m);
    }

    [Fact]
    public async Task GetMarketChartAsync_hits_market_chart_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/market_chart");
            req.RequestUri!.Query.ShouldContain("vs_currency=usd");
            req.RequestUri!.Query.ShouldContain("days=7");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "prices":[[1713916800000,42000.5],[1714003200000,43000.0]],
                        "market_caps":[[1713916800000,800000000000.0]],
                        "total_volumes":[[1713916800000,20000000000.0]]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetMarketChartAsync("bitcoin", "usd", MarketChartRange.SevenDays, ct: TestContext.Current.CancellationToken);
        r.Prices.Count.ShouldBe(2);
        r.Prices[0].Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        r.Prices[0].Value.ShouldBe(42000.5m);
        r.MarketCaps.Count.ShouldBe(1);
        r.TotalVolumes.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetMarketChartRangeAsync_hits_market_chart_range_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/market_chart/range");
            req.RequestUri!.Query.ShouldContain("from=");
            req.RequestUri!.Query.ShouldContain("to=");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "prices":[[1713916800000,42000.5]],
                        "market_caps":[[1713916800000,800000000000.0]],
                        "total_volumes":[[1713916800000,20000000000.0]]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var from = DateTimeOffset.UnixEpoch.AddSeconds(1713916800);
        var to = from.AddDays(7);
        var r = await sut.GetMarketChartRangeAsync("bitcoin", "usd", from, to, ct: TestContext.Current.CancellationToken);
        r.Prices.Count.ShouldBe(1);
        r.Prices[0].Value.ShouldBe(42000.5m);
    }

    [Fact]
    public async Task GetOhlcAsync_hits_ohlc_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/ohlc");
            req.RequestUri!.Query.ShouldContain("vs_currency=usd");
            req.RequestUri!.Query.ShouldContain("days=14");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [
                        [1713916800000,41000.0,43000.0,40500.0,42000.0],
                        [1714003200000,42000.0,44000.0,41500.0,43500.0]
                    ]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetOhlcAsync("bitcoin", "usd", MarketChartRange.FourteenDays, ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        r[0].Open.ShouldBe(41000.0m);
        r[0].High.ShouldBe(43000.0m);
        r[0].Low.ShouldBe(40500.0m);
        r[0].Close.ShouldBe(42000.0m);
    }

    [Fact]
    public async Task GetOhlcRangeAsync_hits_ohlc_range_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/ohlc/range");
            req.RequestUri!.Query.ShouldContain("from=");
            req.RequestUri!.Query.ShouldContain("to=");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [[1713916800000,41000.0,43000.0,40500.0,42000.0]]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var from = DateTimeOffset.UnixEpoch.AddSeconds(1713916800);
        var to = from.AddDays(7);
        var r = await sut.GetOhlcRangeAsync("bitcoin", "usd", from, to, ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Open.ShouldBe(41000.0m);
        r[0].Close.ShouldBe(42000.0m);
    }

    [Fact]
    public async Task GetCirculatingSupplyChartAsync_hits_supply_chart_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/circulating_supply_chart");
            req.RequestUri!.Query.ShouldContain("days=30");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [[1713916800000,"19650000.5"],[1714003200000,"19651000.0"]]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetCirculatingSupplyChartAsync("bitcoin", MarketChartRange.ThirtyDays, ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        r[0].Supply.ShouldBe(19650000.5m);
    }

    [Fact]
    public async Task GetCirculatingSupplyChartRangeAsync_hits_supply_chart_range_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/circulating_supply_chart/range");
            req.RequestUri!.Query.ShouldContain("from=");
            req.RequestUri!.Query.ShouldContain("to=");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [[1713916800000,"19650000.5"]]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var from = DateTimeOffset.UnixEpoch.AddSeconds(1713916800);
        var to = from.AddDays(30);
        var r = await sut.GetCirculatingSupplyChartRangeAsync("bitcoin", from, to, TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Supply.ShouldBe(19650000.5m);
    }

    [Fact]
    public async Task GetTotalSupplyChartAsync_hits_total_supply_chart_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/total_supply_chart");
            req.RequestUri!.Query.ShouldContain("days=90");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [[1713916800000,"21000000.0"]]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetTotalSupplyChartAsync("bitcoin", MarketChartRange.NinetyDays, ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Supply.ShouldBe(21000000.0m);
    }

    [Fact]
    public async Task GetTotalSupplyChartRangeAsync_hits_total_supply_chart_range_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/bitcoin/total_supply_chart/range");
            req.RequestUri!.Query.ShouldContain("from=");
            req.RequestUri!.Query.ShouldContain("to=");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [[1713916800000,"21000000.0"]]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var from = DateTimeOffset.UnixEpoch.AddSeconds(1713916800);
        var to = from.AddDays(90);
        var r = await sut.GetTotalSupplyChartRangeAsync("bitcoin", from, to, TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Supply.ShouldBe(21000000.0m);
    }

    [Fact]
    public async Task GetByContractAsync_hits_contract_endpoint_and_deserializes()
    {
        const string contractAddress = "0x2260fac5e5542a773aa44fbcfedf7c193bc2c599";
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldContain("/coins/ethereum/contract/");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"id":"wrapped-bitcoin","symbol":"wbtc","name":"Wrapped Bitcoin","asset_platform_id":"ethereum"}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetByContractAsync("ethereum", contractAddress, TestContext.Current.CancellationToken);
        r.Id.ShouldBe("wrapped-bitcoin");
        r.AssetPlatformId.ShouldBe("ethereum");
    }

    [Fact]
    public async Task GetContractMarketChartAsync_hits_contract_market_chart_and_deserializes()
    {
        const string contractAddress = "0x2260fac5e5542a773aa44fbcfedf7c193bc2c599";
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldContain("/market_chart");
            req.RequestUri!.Query.ShouldContain("vs_currency=usd");
            req.RequestUri!.Query.ShouldContain("days=7");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "prices":[[1713916800000,42000.5]],
                        "market_caps":[[1713916800000,800000000000.0]],
                        "total_volumes":[[1713916800000,1000000000.0]]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetContractMarketChartAsync("ethereum", contractAddress, "usd", MarketChartRange.SevenDays, ct: TestContext.Current.CancellationToken);
        r.Prices.Count.ShouldBe(1);
        r.Prices[0].Value.ShouldBe(42000.5m);
    }

    [Fact]
    public async Task GetContractMarketChartRangeAsync_hits_contract_market_chart_range_and_deserializes()
    {
        const string contractAddress = "0x2260fac5e5542a773aa44fbcfedf7c193bc2c599";
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldContain("/market_chart/range");
            req.RequestUri!.Query.ShouldContain("from=");
            req.RequestUri!.Query.ShouldContain("to=");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "prices":[[1713916800000,42000.5]],
                        "market_caps":[[1713916800000,800000000000.0]],
                        "total_volumes":[[1713916800000,1000000000.0]]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var from = DateTimeOffset.UnixEpoch.AddSeconds(1713916800);
        var to = from.AddDays(7);
        var r = await sut.GetContractMarketChartRangeAsync("ethereum", contractAddress, "usd", from, to, ct: TestContext.Current.CancellationToken);
        r.Prices.Count.ShouldBe(1);
        r.Prices[0].Value.ShouldBe(42000.5m);
    }

    [Fact]
    public async Task GetTopGainersLosersAsync_hits_top_gainers_losers_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/top_gainers_losers");
            req.RequestUri!.Query.ShouldContain("vs_currency=usd");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "top_gainers":[{"id":"coin-a","symbol":"ca","name":"Coin A","current_price":1.5}],
                        "top_losers":[{"id":"coin-b","symbol":"cb","name":"Coin B","current_price":0.5}]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetTopGainersLosersAsync("usd", ct: TestContext.Current.CancellationToken);
        r.TopGainers.Count.ShouldBe(1);
        r.TopLosers.Count.ShouldBe(1);
        r.TopGainers[0].Id.ShouldBe("coin-a");
        r.TopLosers[0].Id.ShouldBe("coin-b");
    }

    [Fact]
    public async Task GetNewListingsAsync_hits_coins_list_new_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/coins/list/new");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [
                        {"id":"new-coin","symbol":"nc","name":"New Coin","activated_at":1713916800}
                    ]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CoinsClient(http);

        var r = await sut.GetNewListingsAsync(TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("new-coin");
        r[0].ActivatedAt.ShouldBe(1713916800L);
    }
}
