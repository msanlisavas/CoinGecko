using System.Text.Json;
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using CoinGecko.Api.Models.News;
using CoinGecko.Api.Models.Onchain;
using CoinGecko.Api.Resources;
using Microsoft.Extensions.AI;
using NSubstitute;

namespace CoinGecko.Api.AiAgentHub.Tests;

public class NewsAndTopHoldersToolTests
{
    [Fact]
    public async Task get_crypto_news_invokes_news_client_and_projects()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var news = Substitute.For<INewsClient>();
        gecko.News.Returns(news);

        var articles = new[]
        {
            new NewsArticle
            {
                Title = "BTC pumps", Url = "https://x.com/a", Author = "Sat",
                PostedAt = "2026-04-28T09:00:00Z", Type = "news", SourceName = "CryptoDaily",
                RelatedCoinIds = new[] { "bitcoin" },
            },
        };
        news.GetNewsAsync(Arg.Any<NewsOptions?>(), Arg.Any<CancellationToken>())
            .Returns(articles);

        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.News });
        var tool = tools.Single();

        var args = new AIFunctionArguments(new Dictionary<string, object?>
        {
            ["coinId"] = "bitcoin",
            ["language"] = "en",
            ["maxArticles"] = 5,
        });
        var result = await tool.InvokeAsync(args, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.ShouldContain("BTC pumps");
        json.ShouldContain("CryptoDaily");
        json.ShouldContain("bitcoin");
    }

    [Fact]
    public async Task get_top_token_holders_invokes_onchain_and_projects()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var onchain = Substitute.For<IOnchainClient>();
        gecko.Onchain.Returns(onchain);

        var resp = new OnchainTopHolders
        {
            Id = "eth_0xtoken_top_holders",
            Type = "top_holders",
            Attributes = new OnchainTopHoldersAttributes
            {
                LastUpdatedAt = "2026-04-28T10:00:00Z",
                Holders = new[]
                {
                    new OnchainTopHolder
                    {
                        Rank = 1, Address = "0xwhale", Label = "Binance",
                        Amount = 1000m, Percentage = 5m, Value = 50000m,
                        AverageBuyPriceUsd = 40m, UnrealizedPnlUsd = 10000m, RealizedPnlUsd = 200m,
                    },
                },
            },
        };
        onchain.GetTopHoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<OnchainTopHoldersOptions?>(), Arg.Any<CancellationToken>())
               .Returns(resp);

        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.TopHolders, IncludeOnchainTools = true });
        var tool = tools.Single();

        var args = new AIFunctionArguments(new Dictionary<string, object?>
        {
            ["network"] = "eth",
            ["tokenAddress"] = "0xtoken",
            ["topN"] = 10,
            ["includePnl"] = true,
        });
        var result = await tool.InvokeAsync(args, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.ShouldContain("0xwhale");
        json.ShouldContain("Binance");
        json.ShouldContain("10000");
    }

    [Fact]
    public async Task top_holders_tool_is_gated_by_include_onchain_flag()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.TopHolders, IncludeOnchainTools = false });

        tools.ShouldBeEmpty();
        await Task.CompletedTask;
    }
}
