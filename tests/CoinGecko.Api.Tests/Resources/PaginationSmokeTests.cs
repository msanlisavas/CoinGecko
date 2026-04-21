using System.Net;
using System.Text;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class PaginationSmokeTests
{
    private static string MakeMarketsJson(params string[] ids)
    {
        var items = string.Join(",", Array.ConvertAll(ids, id => $$$"""{"id":"{{{id}}}","symbol":"{{{id}}}","name":"{{{id}}}"}"""));
        return $"[{items}]";
    }

    [Fact]
    public async Task EnumerateMarketsAsync_walks_pages_and_stops_on_short_page()
    {
        // Page 1 has 2 items (perPage=2 → full), page 2 has 1 item (short → stop).
        var callCount = 0;
        var stub = new StubHandler((req, _) =>
        {
            callCount++;
            var query = req.RequestUri!.Query;
            var json = query.Contains("&page=2") || query.StartsWith("?page=2", StringComparison.Ordinal)
                ? MakeMarketsJson("coin-c")
                : MakeMarketsJson("coin-a", "coin-b");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var client = new CoinsClient(http);

        var collected = new List<CoinMarket>();
        await foreach (var item in client.EnumerateMarketsAsync(
            "usd",
            new CoinMarketsOptions { PerPage = 2 },
            TestContext.Current.CancellationToken))
        {
            collected.Add(item);
        }

        collected.Count.ShouldBe(3);
        collected[0].Id.ShouldBe("coin-a");
        collected[1].Id.ShouldBe("coin-b");
        collected[2].Id.ShouldBe("coin-c");
        callCount.ShouldBe(2);
    }
}
