using CoinGecko.Api.AiAgentHub;

namespace CoinGecko.Api.AiAgentHub.Tests;

public class OptionsTests
{
    [Fact]
    public void ToolSet_has_expected_flags_and_All()
    {
        CoinGeckoToolSet.CoinPrices.ShouldBe((CoinGeckoToolSet)1);
        CoinGeckoToolSet.CoinSearch.ShouldBe((CoinGeckoToolSet)2);
        CoinGeckoToolSet.MarketData.ShouldBe((CoinGeckoToolSet)4);
        CoinGeckoToolSet.Trending.ShouldBe((CoinGeckoToolSet)8);
        CoinGeckoToolSet.Categories.ShouldBe((CoinGeckoToolSet)16);
        CoinGeckoToolSet.Nfts.ShouldBe((CoinGeckoToolSet)32);
        CoinGeckoToolSet.Derivatives.ShouldBe((CoinGeckoToolSet)64);
        CoinGeckoToolSet.Onchain.ShouldBe((CoinGeckoToolSet)128);

        CoinGeckoToolSet.All.HasFlag(CoinGeckoToolSet.CoinPrices).ShouldBeTrue();
        CoinGeckoToolSet.All.HasFlag(CoinGeckoToolSet.Onchain).ShouldBeTrue();
    }

    [Fact]
    public void Options_defaults()
    {
        var o = new CoinGeckoAiToolsOptions();
        o.Tools.ShouldBe(CoinGeckoToolSet.All);
        o.MaxResults.ShouldBe(25);
        o.IncludeOnchainTools.ShouldBeTrue();
        o.ToolFilter.ShouldBeNull();
    }
}
