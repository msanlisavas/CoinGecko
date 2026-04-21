using System.Text.Json;
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using CoinGecko.Api.AiAgentHub.Projections;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using Microsoft.Extensions.AI;
using NSubstitute;

namespace CoinGecko.Api.AiAgentHub.Tests;

public class ToolInvocationTests
{
    private static readonly string[] BitcoinIdArray = ["bitcoin"];

    [Fact]
    public async Task get_coin_prices_invokes_simple_client_and_projects()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var simple = Substitute.For<ISimpleClient>();
        gecko.Simple.Returns(simple);

        var priceMap = new Dictionary<string, IReadOnlyDictionary<string, decimal?>>
        {
            ["bitcoin"] = new Dictionary<string, decimal?>
            {
                ["usd"] = 42000m,
                ["usd_24h_change"] = 1.23m,
                ["usd_market_cap"] = 800_000_000_000m,
            },
        };
        simple.GetPriceAsync(Arg.Any<SimplePriceOptions>(), Arg.Any<CancellationToken>())
              .Returns((IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal?>>)priceMap);

        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.CoinPrices });
        var tool = tools.Single();

        var args = new AIFunctionArguments(new Dictionary<string, object?>
        {
            ["coinIds"] = BitcoinIdArray,
            ["vsCurrency"] = "usd",
        });
        var result = await tool.InvokeAsync(args, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        // MEAI wraps the return value as a JsonElement; serialize it to verify content.
        var json = JsonSerializer.Serialize(result);
        json.ShouldContain("bitcoin");
        json.ShouldContain("42000");
    }

    [Fact]
    public async Task coin_search_invokes_search_client_and_projects()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var search = Substitute.For<ISearchClient>();
        gecko.Search.Returns(search);

        var searchResults = new SearchResults
        {
            Coins = new[]
            {
                new SearchCoinHit { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", MarketCapRank = 1 },
            },
        };
        search.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(searchResults);

        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.CoinSearch });
        var tool = tools.Single();

        var args = new AIFunctionArguments(new Dictionary<string, object?>
        {
            ["query"] = "bitcoin",
            ["maxResults"] = 5,
        });
        var result = await tool.InvokeAsync(args, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.ShouldContain("bitcoin");
    }

    [Fact]
    public async Task get_top_markets_invokes_coins_client_and_projects()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var coins = Substitute.For<ICoinsClient>();
        gecko.Coins.Returns(coins);

        var markets = new[]
        {
            new CoinMarket
            {
                Id = "bitcoin", Symbol = "btc", Name = "Bitcoin",
                CurrentPrice = 42000m, MarketCapRank = 1,
                MarketCap = 800_000_000_000m, TotalVolume = 20_000_000_000m,
                PriceChangePercentage24h = 1.5m,
            },
        };
        coins.GetMarketsAsync(Arg.Any<string>(), Arg.Any<CoinMarketsOptions?>(), Arg.Any<CancellationToken>())
             .Returns((IReadOnlyList<CoinMarket>)markets);

        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.MarketData });
        var tool = tools.Single();

        var args = new AIFunctionArguments(new Dictionary<string, object?>
        {
            ["vsCurrency"] = "usd",
            ["limit"] = 10,
        });
        var result = await tool.InvokeAsync(args, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.ShouldContain("bitcoin");
        json.ShouldContain("42000");
    }
}
