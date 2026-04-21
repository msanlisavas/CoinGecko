using System.Net;
using System.Text;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class NftsClientTests
{
    [Fact]
    public async Task GetListAsync_hits_nfts_list_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/nfts/list");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [
                        {"id":"cryptopunks","name":"CryptoPunks","asset_platform_id":"ethereum","contract_address":"0xb47e3cd837dDF8e4c57F05d70Ab865de6e193BBB","symbol":"PUNK"}
                    ]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new NftsClient(http);

        var r = await sut.GetListAsync(ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("cryptopunks");
        r[0].Name.ShouldBe("CryptoPunks");
        r[0].AssetPlatformId.ShouldBe("ethereum");
    }

    [Fact]
    public async Task GetAsync_hits_nfts_by_id_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/nfts/cryptopunks");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "id":"cryptopunks",
                        "name":"CryptoPunks",
                        "native_currency":"ethereum",
                        "native_currency_symbol":"ETH",
                        "total_supply":"10000"
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new NftsClient(http);

        var r = await sut.GetAsync("cryptopunks", TestContext.Current.CancellationToken);
        r.Id.ShouldBe("cryptopunks");
        r.NativeCurrencySymbol.ShouldBe("ETH");
        r.TotalSupply.ShouldBe(10000m);
    }

    [Fact]
    public async Task GetByContractAsync_hits_nfts_contract_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldContain("/nfts/ethereum/contract/");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "id":"cryptopunks",
                        "contract_address":"0xb47e3cd837dDF8e4c57F05d70Ab865de6e193BBB",
                        "asset_platform_id":"ethereum",
                        "name":"CryptoPunks"
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new NftsClient(http);

        var r = await sut.GetByContractAsync(
            "ethereum", "0xb47e3cd837dDF8e4c57F05d70Ab865de6e193BBB",
            TestContext.Current.CancellationToken);
        r.Id.ShouldBe("cryptopunks");
        r.ContractAddress.ShouldBe("0xb47e3cd837dDF8e4c57F05d70Ab865de6e193BBB");
    }

    [Fact]
    public async Task GetMarketsAsync_hits_nfts_markets_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/nfts/markets");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [
                        {"id":"cryptopunks","name":"CryptoPunks","asset_platform_id":"ethereum","last_updated_at":"2024-04-23T00:00:00Z"}
                    ]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new NftsClient(http);

        var r = await sut.GetMarketsAsync(ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("cryptopunks");
        r[0].LastUpdatedAt.ShouldBe("2024-04-23T00:00:00Z");
    }

    [Fact]
    public async Task GetMarketChartAsync_hits_nft_market_chart_and_merges_points()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/nfts/cryptopunks/market_chart");
            req.RequestUri!.Query.ShouldContain("days=30");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "floor_price_usd":    [[1713916800000, 5.2], [1714003200000, 5.5]],
                        "floor_price_native": [[1713916800000, 0.001], [1714003200000, 0.0011]],
                        "market_cap_native":  [[1713916800000, 10.0], [1714003200000, 11.0]],
                        "h24_volume_native":  [[1713916800000, 1.5], [1714003200000, 1.6]]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new NftsClient(http);

        var r = await sut.GetMarketChartAsync("cryptopunks", 30, TestContext.Current.CancellationToken);
        r.Count.ShouldBe(2);
        r[0].Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1713916800000L));
        r[0].FloorPriceNative.ShouldBe(0.001m);
        r[0].FloorPriceUsd.ShouldBe(5.2m);
        r[0].MarketCapNative.ShouldBe(10.0m);
        r[0].Volume24hNative.ShouldBe(1.5m);
    }

    [Fact]
    public async Task GetTickersAsync_hits_nft_tickers_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/nfts/cryptopunks/tickers");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "tickers":[
                            {
                                "floor_price_in_native_currency":75.5,
                                "native_currency":"eth",
                                "nft_marketplace_id":"opensea",
                                "name":"OpenSea"
                            }
                        ]
                    }
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new NftsClient(http);

        var r = await sut.GetTickersAsync("cryptopunks", TestContext.Current.CancellationToken);
        r.Tickers.Count.ShouldBe(1);
        r.Tickers[0].NftMarketplaceId.ShouldBe("opensea");
        r.Tickers[0].FloorPriceInNativeCurrency.ShouldBe(75.5m);
        r.Tickers[0].NativeCurrency.ShouldBe("eth");
    }
}
