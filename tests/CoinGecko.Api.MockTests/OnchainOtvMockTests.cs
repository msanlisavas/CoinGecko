using CoinGecko.Api.MockTests.Infra;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CoinGecko.Api.MockTests;

public class OnchainOtvMockTests : IClassFixture<CoinGeckoMockFixture>
{
    private readonly CoinGeckoMockFixture _fx;

    public OnchainOtvMockTests(CoinGeckoMockFixture fx) => _fx = fx;

    [Fact]
    public async Task Token_response_deserializes_otv_and_gt_verified_fields()
    {
        _fx.Server
            .Given(Request.Create().WithPath("/api/v3/onchain/networks/eth/tokens/0xabc").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                  "data": {
                    "id": "eth_0xabc",
                    "type": "token",
                    "attributes": {
                      "address": "0xabc",
                      "name": "Test",
                      "symbol": "TEST",
                      "outstanding_supply": "1000000",
                      "outstanding_token_value_usd": "5000000.42",
                      "gt_verified": true
                    }
                  }
                }
                """));

        var token = await _fx.Client.Onchain.GetTokenAsync("eth", "0xabc", ct: TestContext.Current.CancellationToken);

        token.Attributes!.OutstandingSupply.ShouldBe(1_000_000m);
        token.Attributes!.OutstandingTokenValueUsd.ShouldBe(5_000_000.42m);
        token.Attributes!.GtVerified.ShouldBe(true);
    }

    [Fact]
    public async Task Pool_response_deserializes_gt_verified_field()
    {
        _fx.Server
            .Given(Request.Create().WithPath("/api/v3/onchain/networks/eth/pools/0xpool").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                  "data": {
                    "id": "eth_0xpool",
                    "type": "pool",
                    "attributes": {
                      "name": "WETH / USDC",
                      "address": "0xpool",
                      "gt_verified": false
                    }
                  }
                }
                """));

        var pool = await _fx.Client.Onchain.GetPoolAsync("eth", "0xpool", ct: TestContext.Current.CancellationToken);

        pool.Attributes!.GtVerified.ShouldBe(false);
    }
}
