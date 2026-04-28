using CoinGecko.Api.MockTests.Infra;
using CoinGecko.Api.Models.Onchain;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CoinGecko.Api.MockTests;

public class OnchainTopHoldersMockTests : IClassFixture<CoinGeckoPaidMockFixture>
{
    private readonly CoinGeckoPaidMockFixture _fx;

    public OnchainTopHoldersMockTests(CoinGeckoPaidMockFixture fx) => _fx = fx;

    [Fact]
    public async Task Without_pnl_details_pnl_fields_are_null()
    {
        _fx.Server
            .Given(Request.Create()
                .WithPath("/api/v3/onchain/networks/eth/tokens/0xtoken/top_holders")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                  "data": {
                    "id": "eth_0xtoken_top_holders",
                    "type": "top_holders",
                    "attributes": {
                      "last_updated_at": "2026-04-28T10:00:00Z",
                      "holders": [
                        {
                          "rank": 1,
                          "address": "0xwhale",
                          "label": "Binance Hot Wallet",
                          "amount": "12345.6789",
                          "percentage": "12.5",
                          "value": "5000000.00",
                          "explorer_url": "https://etherscan.io/address/0xwhale"
                        }
                      ]
                    }
                  }
                }
                """));

        var resp = await _fx.Client.Onchain.GetTopHoldersAsync("eth", "0xtoken", ct: TestContext.Current.CancellationToken);

        resp.Attributes!.LastUpdatedAt.ShouldBe("2026-04-28T10:00:00Z");
        resp.Attributes.Holders!.Count.ShouldBe(1);
        var h = resp.Attributes.Holders[0];
        h.Rank.ShouldBe(1);
        h.Address.ShouldBe("0xwhale");
        h.Label.ShouldBe("Binance Hot Wallet");
        h.Amount.ShouldBe(12345.6789m);
        h.Percentage.ShouldBe(12.5m);
        h.Value.ShouldBe(5_000_000m);
        h.ExplorerUrl.ShouldBe("https://etherscan.io/address/0xwhale");
        h.AverageBuyPriceUsd.ShouldBeNull();
        h.RealizedPnlUsd.ShouldBeNull();
        h.UnrealizedPnlUsd.ShouldBeNull();
        h.TotalBuyCount.ShouldBeNull();
        h.TotalSellCount.ShouldBeNull();
    }

    [Fact]
    public async Task With_pnl_details_query_param_is_sent_and_pnl_fields_deserialize()
    {
        _fx.Server
            .Given(Request.Create()
                .WithPath("/api/v3/onchain/networks/eth/tokens/0xtoken/top_holders")
                .WithParam("include_pnl_details", "true")
                .WithParam("holders", "5")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                  "data": {
                    "id": "eth_0xtoken_top_holders",
                    "type": "top_holders",
                    "attributes": {
                      "last_updated_at": "2026-04-28T10:00:00Z",
                      "holders": [
                        {
                          "rank": 1,
                          "address": "0xwhale",
                          "label": null,
                          "amount": "1000",
                          "percentage": "5.0",
                          "value": "10000",
                          "average_buy_price_usd": "8.5",
                          "total_buy_count": 42,
                          "total_sell_count": 7,
                          "unrealized_pnl_usd": "1500.25",
                          "unrealized_pnl_percentage": "15.0",
                          "realized_pnl_usd": "200.75",
                          "realized_pnl_percentage": "2.5",
                          "explorer_url": "https://etherscan.io/address/0xwhale"
                        }
                      ]
                    }
                  }
                }
                """));

        var opts = new OnchainTopHoldersOptions { IncludePnlDetails = true, Holders = "5" };
        var resp = await _fx.Client.Onchain.GetTopHoldersAsync("eth", "0xtoken", opts, TestContext.Current.CancellationToken);

        var h = resp.Attributes!.Holders![0];
        h.AverageBuyPriceUsd.ShouldBe(8.5m);
        h.TotalBuyCount.ShouldBe(42);
        h.TotalSellCount.ShouldBe(7);
        h.UnrealizedPnlUsd.ShouldBe(1500.25m);
        h.UnrealizedPnlPercentage.ShouldBe(15.0m);
        h.RealizedPnlUsd.ShouldBe(200.75m);
        h.RealizedPnlPercentage.ShouldBe(2.5m);
        h.Label.ShouldBeNull();
    }
}
