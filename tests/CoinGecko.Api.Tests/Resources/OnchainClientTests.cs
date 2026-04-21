using System.Net;
using System.Text;
using CoinGecko.Api.Models.Onchain;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class OnchainClientTests
{
    private static OnchainClient CreateSut(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
    {
        var stub = new StubHandler(handler);
        var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        return new OnchainClient(http);
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    [Fact]
    public async Task GetNetworksAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks");
            return Json("""{"data":[{"id":"eth","type":"network","attributes":{"name":"Ethereum","coingecko_asset_platform_id":"ethereum"}}]}""");
        });

        var result = await sut.GetNetworksAsync(ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("eth");
        result[0].Attributes!.Name.ShouldBe("Ethereum");
    }

    [Fact]
    public async Task GetNetworksAsync_sends_page_param()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.Query.ShouldContain("page=2");
            return Json("""{"data":[]}""");
        });

        var result = await sut.GetNetworksAsync(page: 2, ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(0);
    }

    [Fact]
    public async Task GetDexesAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/dexes");
            return Json("""{"data":[{"id":"uniswap_v3","type":"dex","attributes":{"name":"Uniswap V3"}}]}""");
        });

        var result = await sut.GetDexesAsync("eth", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("uniswap_v3");
        result[0].Attributes!.Name.ShouldBe("Uniswap V3");
    }

    [Fact]
    public async Task GetPoolAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/pools/0xabc");
            return Json("""{"data":{"id":"eth_0xabc","type":"pool","attributes":{"name":"WETH / USDC","address":"0xabc","reserve_in_usd":"1000000"}}}""");
        });

        var result = await sut.GetPoolAsync("eth", "0xabc", ct: TestContext.Current.CancellationToken);

        result.Id.ShouldBe("eth_0xabc");
        result.Attributes!.Name.ShouldBe("WETH / USDC");
        result.Attributes.ReserveInUsd.ShouldBe(1_000_000m);
    }

    [Fact]
    public async Task GetPoolsMultiAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldContain("/onchain/networks/eth/pools/multi/");
            return Json("""{"data":[{"id":"eth_0xabc","type":"pool","attributes":{"name":"WETH / USDC","address":"0xabc"}},{"id":"eth_0xdef","type":"pool","attributes":{"name":"WBTC / ETH","address":"0xdef"}}]}""");
        });

        var result = await sut.GetPoolsMultiAsync("eth", (IReadOnlyList<string>)["0xabc", "0xdef"], ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(2);
        result[0].Attributes!.Name.ShouldBe("WETH / USDC");
        result[1].Id.ShouldBe("eth_0xdef");
    }

    [Fact]
    public async Task GetTrendingPoolsAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/trending_pools");
            return Json("""{"data":[{"id":"eth_0xtrend","type":"pool","attributes":{"name":"PEPE / WETH","address":"0xtrend"}}]}""");
        });

        var result = await sut.GetTrendingPoolsAsync(ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Name.ShouldBe("PEPE / WETH");
    }

    [Fact]
    public async Task GetTrendingPoolsByNetworkAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/trending_pools");
            return Json("""{"data":[{"id":"eth_0xtrend","type":"pool","attributes":{"name":"PEPE / WETH","address":"0xtrend"}}]}""");
        });

        var result = await sut.GetTrendingPoolsByNetworkAsync("eth", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("eth_0xtrend");
    }

    [Fact]
    public async Task GetTopPoolsByNetworkAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/pools");
            return Json("""{"data":[{"id":"eth_0xtop","type":"pool","attributes":{"name":"WETH / USDC","address":"0xtop"}}]}""");
        });

        var result = await sut.GetTopPoolsByNetworkAsync("eth", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Address.ShouldBe("0xtop");
    }

    [Fact]
    public async Task GetTopPoolsByDexAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/dexes/uniswap_v3/pools");
            return Json("""{"data":[{"id":"eth_0xpool","type":"pool","attributes":{"name":"WETH / USDC","address":"0xpool"}}]}""");
        });

        var result = await sut.GetTopPoolsByDexAsync("eth", "uniswap_v3", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Name.ShouldBe("WETH / USDC");
    }

    [Fact]
    public async Task GetNewPoolsAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/new_pools");
            return Json("""{"data":[{"id":"bsc_0xnew","type":"pool","attributes":{"name":"TOKEN / BNB","address":"0xnew"}}]}""");
        });

        var result = await sut.GetNewPoolsAsync(ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("bsc_0xnew");
    }

    [Fact]
    public async Task GetNewPoolsByNetworkAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/bsc/new_pools");
            return Json("""{"data":[{"id":"bsc_0xnew","type":"pool","attributes":{"name":"TOKEN / BNB","address":"0xnew"}}]}""");
        });

        var result = await sut.GetNewPoolsByNetworkAsync("bsc", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Address.ShouldBe("0xnew");
    }

    [Fact]
    public async Task GetPoolsMegafilterAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/pools/megafilter");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Analyst);
            return Json("""{"data":[{"id":"eth_0xmega","type":"pool","attributes":{"name":"MEGA / WETH","address":"0xmega"}}]}""");
        });

        var result = await sut.GetPoolsMegafilterAsync(new OnchainMegafilterOptions { Networks = (IReadOnlyList<string>)["eth"] }, TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Name.ShouldBe("MEGA / WETH");
    }

    [Fact]
    public async Task GetTrendingSearchPoolsAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/pools/trending_search");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":[{"id":"eth_0xts","type":"pool","attributes":{"name":"HOT / WETH","address":"0xts"}}]}""");
        });

        var result = await sut.GetTrendingSearchPoolsAsync(TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("eth_0xts");
    }

    [Fact]
    public async Task GetPoolInfoAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/pools/0xabc/info");
            return Json("""{"data":{"base_token":{"id":"eth_0xbase","type":"token","attributes":{"name":"Wrapped Ether","symbol":"WETH"}},"quote_token":{"id":"eth_0xquote","type":"token","attributes":{"name":"USD Coin","symbol":"USDC"}}}}""");
        });

        var result = await sut.GetPoolInfoAsync("eth", "0xabc", TestContext.Current.CancellationToken);

        result.BaseToken!.Id.ShouldBe("eth_0xbase");
        result.QuoteToken!.Id.ShouldBe("eth_0xquote");
    }

    [Fact]
    public async Task GetPoolOhlcvAsync_hits_path_and_projects_rows()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/pools/0xabc/ohlcv/hour");
            return Json("""{"data":{"id":"eth_0xabc_hour","type":"ohlcv","attributes":{"ohlcv_list":[[1700000000,1800.5,1850.0,1780.0,1820.0,500000.0]]}}}""");
        });

        var result = await sut.GetPoolOhlcvAsync("eth", "0xabc", OnchainTimeframe.Hour, ct: TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Open.ShouldBe(1800.5m);
        result[0].High.ShouldBe(1850.0m);
        result[0].VolumeUsd.ShouldBe(500000.0m);
        result[0].Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeSeconds(1700000000));
    }

    [Fact]
    public async Task GetPoolTradesAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/pools/0xabc/trades");
            return Json("""{"data":[{"id":"trade_1","type":"trade","attributes":{"block_number":18000000,"tx_hash":"0xtxhash","kind":"buy","volume_in_usd":"1500.50"}}]}""");
        });

        var result = await sut.GetPoolTradesAsync("eth", "0xabc", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("trade_1");
        var attrs = result[0].Attributes!;
        attrs.Kind.ShouldBe("buy");
        attrs.VolumeInUsd.ShouldBe(1500.50m);
    }
}
