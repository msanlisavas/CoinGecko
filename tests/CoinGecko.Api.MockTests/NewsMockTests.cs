using CoinGecko.Api.MockTests.Infra;
using CoinGecko.Api.Models.News;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CoinGecko.Api.MockTests;

public class NewsMockTests : IClassFixture<CoinGeckoPaidMockFixture>
{
    private readonly CoinGeckoPaidMockFixture _fx;

    public NewsMockTests(CoinGeckoPaidMockFixture fx) => _fx = fx;

    [Fact]
    public async Task GetNews_default_request_deserializes_array()
    {
        _fx.Server
            .Given(Request.Create().WithPath("/api/v3/news").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                [
                  {
                    "title": "Bitcoin hits new all-time high",
                    "url": "https://example.com/btc-ath",
                    "image": "https://example.com/img.png",
                    "author": "Satoshi N.",
                    "posted_at": "2026-04-28T09:00:00Z",
                    "type": "news",
                    "source_name": "CryptoDaily",
                    "related_coin_ids": ["bitcoin"]
                  }
                ]
                """));

        var articles = await _fx.Client.News.GetNewsAsync(ct: TestContext.Current.CancellationToken);

        articles.Length.ShouldBe(1);
        articles[0].Title.ShouldBe("Bitcoin hits new all-time high");
        articles[0].SourceName.ShouldBe("CryptoDaily");
        articles[0].Type.ShouldBe("news");
        articles[0].RelatedCoinIds!.Count.ShouldBe(1);
        articles[0].RelatedCoinIds![0].ShouldBe("bitcoin");
    }

    [Fact]
    public async Task GetNews_passes_all_query_parameters()
    {
        _fx.Server
            .Given(Request.Create()
                .WithPath("/api/v3/news")
                .WithParam("page", "2")
                .WithParam("per_page", "5")
                .WithParam("coin_id", "ethereum")
                .WithParam("language", "ja")
                .WithParam("type", "guides")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[]"));

        var opts = new NewsOptions
        {
            Page = 2,
            PerPage = 5,
            CoinId = "ethereum",
            Language = "ja",
            Type = "guides",
        };

        var articles = await _fx.Client.News.GetNewsAsync(opts, TestContext.Current.CancellationToken);

        articles.ShouldBeEmpty();
    }
}
