using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class AssetPlatformsClientTests
{
    [Fact]
    public async Task GetListAsync_hits_asset_platforms_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/asset_platforms");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    [{"id":"ethereum","chain_identifier":1,"name":"Ethereum","shortname":"ETH","native_coin_id":"ethereum"}]
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new AssetPlatformsClient(http);

        var r = await sut.GetListAsync(ct: TestContext.Current.CancellationToken);
        r.Count.ShouldBe(1);
        r[0].Id.ShouldBe("ethereum");
        r[0].ChainIdentifier.ShouldBe(1L);
    }

    [Fact]
    public async Task GetTokenListAsync_hits_token_lists_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldContain("token_lists/ethereum/all.json");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"name":"CoinGecko","timestamp":"2024-01-01T00:00:00Z","version":{"major":1,"minor":0,"patch":0},"tokens":[{"chainId":1,"address":"0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48","name":"USD Coin","symbol":"USDC","decimals":6}]}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new AssetPlatformsClient(http);

        var r = await sut.GetTokenListAsync("ethereum", TestContext.Current.CancellationToken);
        r.Name.ShouldBe("CoinGecko");
        r.Tokens!.Count.ShouldBe(1);
        r.Tokens[0].Symbol.ShouldBe("USDC");
    }
}
