using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using Microsoft.Extensions.AI;
using NSubstitute;

namespace CoinGecko.Api.AiAgentHub.Tests;

public class ToolsFactoryTests
{
    [Fact]
    public void Create_all_flags_returns_all_tools()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko);
        tools.Count.ShouldBe(11);
        tools.Select(t => t.Name).ShouldContain("get_coin_prices");
        tools.Select(t => t.Name).ShouldContain("get_top_markets");
        tools.Select(t => t.Name).ShouldContain("search_onchain_pools");
        tools.Select(t => t.Name).ShouldContain("get_top_token_holders");
        tools.Select(t => t.Name).ShouldContain("get_crypto_news");
    }

    [Fact]
    public void Create_filtered_to_coin_prices_returns_single_tool()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.CoinPrices });
        tools.Count.ShouldBe(1);
        tools[0].Name.ShouldBe("get_coin_prices");
    }

    [Fact]
    public void Create_includeOnchainTools_false_hides_onchain_tools()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { IncludeOnchainTools = false });
        tools.Count.ShouldBe(8); // 11 - 3 onchain (search_pools, token_prices, top_holders)
        tools.Select(t => t.Name).ShouldNotContain("search_onchain_pools");
        tools.Select(t => t.Name).ShouldNotContain("get_onchain_token_prices");
        tools.Select(t => t.Name).ShouldNotContain("get_top_token_holders");
    }

    [Fact]
    public void Create_custom_filter_applied()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko, new CoinGeckoAiToolsOptions
        {
            ToolFilter = name => name.StartsWith("get_", StringComparison.Ordinal),
        });
        tools.All(t => t.Name.StartsWith("get_", StringComparison.Ordinal)).ShouldBeTrue();
        tools.Select(t => t.Name).ShouldNotContain("coin_search");
        tools.Select(t => t.Name).ShouldNotContain("search_onchain_pools");
    }
}
