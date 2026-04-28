using System.Net;
using System.Text;
using CoinGecko.Api.Models.Onchain;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class OnchainClientBatch5bTests
{
    private static OnchainClient CreateSut(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
    {
        var stub = new StubHandler(handler);
        var http = new HttpClient(stub) { BaseAddress = new Uri("https://pro-api.coingecko.com/api/v3/") };
        return new OnchainClient(http);
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    [Fact]
    public async Task GetTokenPriceAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldContain("/onchain/simple/networks/eth/token_price/");
            return Json("""{"data":{"id":"eth_prices","type":"simple_token_price","attributes":{"token_prices":{"0xabc":1.23,"0xdef":4.56}}}}""");
        });

        var result = await sut.GetTokenPriceAsync("eth", (IReadOnlyList<string>)["0xabc", "0xdef"], options: null, TestContext.Current.CancellationToken);

        result.Attributes!.TokenPrices!["0xabc"].ShouldBe(1.23m);
        result.Attributes.TokenPrices["0xdef"].ShouldBe(4.56m);
    }

    [Fact]
    public async Task SearchPoolsAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/search/pools");
            req.RequestUri.Query.ShouldContain("query=WETH");
            return Json("""{"data":[{"id":"eth_0xsearch","type":"pool","attributes":{"name":"WETH / USDC","address":"0xsearch"}}]}""");
        });

        var result = await sut.SearchPoolsAsync("WETH", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Name.ShouldBe("WETH / USDC");
    }

    [Fact]
    public async Task GetTokenAsync_hits_token_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc");
            return Json("""{"data":{"id":"eth_0xabc","type":"token","attributes":{"address":"0xabc","name":"Test Token","symbol":"T","decimals":18,"price_usd":1.23}}}""");
        });

        var result = await sut.GetTokenAsync("eth", "0xabc", options: null, TestContext.Current.CancellationToken);

        result.Id.ShouldBe("eth_0xabc");
        result.Attributes!.Symbol.ShouldBe("T");
        result.Attributes.PriceUsd.ShouldBe(1.23m);
    }

    [Fact]
    public async Task GetTokensMultiAsync_hits_multi_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldContain("/onchain/networks/eth/tokens/multi/");
            return Json("""{"data":[{"id":"eth_0xabc","type":"token","attributes":{"address":"0xabc","symbol":"AA"}},{"id":"eth_0xdef","type":"token","attributes":{"address":"0xdef","symbol":"BB"}}]}""");
        });

        var result = await sut.GetTokensMultiAsync("eth", (IReadOnlyList<string>)["0xabc", "0xdef"], options: null, TestContext.Current.CancellationToken);

        result.Length.ShouldBe(2);
        result[0].Attributes!.Symbol.ShouldBe("AA");
        result[1].Attributes!.Symbol.ShouldBe("BB");
    }

    [Fact]
    public async Task GetPoolsByTokenAsync_hits_pools_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc/pools");
            return Json("""{"data":[{"id":"eth_0xpool","type":"pool","attributes":{"name":"Test / USDC","address":"0xpool"}}]}""");
        });

        var result = await sut.GetPoolsByTokenAsync("eth", "0xabc", options: null, TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Name.ShouldBe("Test / USDC");
    }

    [Fact]
    public async Task GetTokenInfoAsync_hits_info_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc/info");
            return Json("""{"data":{"id":"eth_0xabc","type":"token_info","attributes":{"address":"0xabc","name":"My Token","symbol":"MT","twitter_handle":"mytoken"}}}""");
        });

        var result = await sut.GetTokenInfoAsync("eth", "0xabc", TestContext.Current.CancellationToken);

        result.Id.ShouldBe("eth_0xabc");
        result.Attributes!.TwitterHandle.ShouldBe("mytoken");
    }

    [Fact]
    public async Task GetRecentlyUpdatedTokensAsync_hits_path_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/tokens/info_recently_updated");
            return Json("""{"data":[{"id":"eth_0xrecent","type":"token_info","attributes":{"address":"0xrecent","name":"Recent","symbol":"RCT"}}]}""");
        });

        var result = await sut.GetRecentlyUpdatedTokensAsync(ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Symbol.ShouldBe("RCT");
    }

    [Fact]
    public async Task GetTokenOhlcvAsync_hits_path_sets_plan_and_projects_rows()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc/ohlcv/hour");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":{"id":"eth_0xabc_hour","type":"ohlcv","attributes":{"ohlcv_list":[[1700000000,1800.5,1850.0,1780.0,1820.0,500000.0]]}}}""");
        });

        var result = await sut.GetTokenOhlcvAsync("eth", "0xabc", OnchainTimeframe.Hour, ct: TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Open.ShouldBe(1800.5m);
        result[0].VolumeUsd.ShouldBe(500000.0m);
    }

    [Fact]
    public async Task GetTokenTradesAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc/trades");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":[{"id":"trade_1","type":"trade","attributes":{"block_number":18000000,"tx_hash":"0xtx","kind":"sell","volume_in_usd":"999.99"}}]}""");
        });

        var result = await sut.GetTokenTradesAsync("eth", "0xabc", ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        var tradeAttrs = result[0].Attributes!;
        tradeAttrs.Kind.ShouldBe("sell");
        tradeAttrs.VolumeInUsd.ShouldBe(999.99m);
    }

    [Fact]
    public async Task GetTopTradersAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc/top_traders");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":[{"id":"0xwallet","type":"trader","attributes":{"address":"0xwallet"}}]}""");
        });

        var result = await sut.GetTopTradersAsync("eth", "0xabc", TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("0xwallet");
    }

    [Fact]
    public async Task GetTopHoldersAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc/top_holders");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":{"id":"eth_0xabc_top_holders","type":"top_holders","attributes":{"last_updated_at":"2026-04-28T10:00:00Z","holders":[{"rank":1,"address":"0xholder","amount":"1000","percentage":"5.5"}]}}}""");
        });

        var result = await sut.GetTopHoldersAsync("eth", "0xabc", ct: TestContext.Current.CancellationToken);

        result.Id.ShouldBe("eth_0xabc_top_holders");
        result.Attributes!.Holders!.Count.ShouldBe(1);
        result.Attributes.Holders[0].Address.ShouldBe("0xholder");
        result.Attributes.Holders[0].Amount.ShouldBe(1000m);
    }

    [Fact]
    public async Task GetHoldersChartAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/networks/eth/tokens/0xabc/holders_chart");
            req.RequestUri.Query.ShouldContain("days=30");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":{"id":"eth_0xabc_holders","type":"holders_chart","attributes":{"token_holders_list":[[1700000000,5000],[1700086400,5100]]}}}""");
        });

        var result = await sut.GetHoldersChartAsync("eth", "0xabc", 30, TestContext.Current.CancellationToken);

        result.Attributes!.TokenHoldersList!.Length.ShouldBe(2);
        result.Attributes.TokenHoldersList[0][1].ShouldBe(5000m);
    }

    [Fact]
    public async Task GetCategoriesAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/categories");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":[{"id":"defi","type":"category","attributes":{"name":"DeFi","pools_count":42}}]}""");
        });

        var result = await sut.GetCategoriesAsync(ct: TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("defi");
        var catAttrs = result[0].Attributes!;
        catAttrs.Name.ShouldBe("DeFi");
        catAttrs.PoolsCount.ShouldBe(42);
    }

    [Fact]
    public async Task GetCategoryPoolsAsync_hits_path_sets_plan_and_deserializes()
    {
        var sut = CreateSut((req, _) =>
        {
            req.RequestUri!.AbsolutePath.ShouldEndWith("/onchain/categories/defi/pools");
            req.Options.TryGetValue(new HttpRequestOptionsKey<CoinGeckoPlan?>("coingecko.required_plan"), out var plan);
            plan.ShouldBe(CoinGeckoPlan.Basic);
            return Json("""{"data":[{"id":"eth_0xcatpool","type":"pool","attributes":{"name":"DeFi Pool","address":"0xcatpool"}}]}""");
        });

        var result = await sut.GetCategoryPoolsAsync("defi", options: null, TestContext.Current.CancellationToken);

        result.Length.ShouldBe(1);
        result[0].Attributes!.Name.ShouldBe("DeFi Pool");
    }
}
