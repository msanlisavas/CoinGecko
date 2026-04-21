using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class KeyClientTests
{
    [Fact]
    public async Task GetAsync_hits_key_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldEndWith("/key");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"plan":"Analyst","rate_limit_request_per_minute":500,"monthly_call_credit":10000000,"current_total_monthly_calls":12345,"current_remaining_monthly_calls":9987655}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new KeyClient(http);

        var r = await sut.GetAsync(TestContext.Current.CancellationToken);
        r.Plan.ShouldBe("Analyst");
        r.RateLimitRequestPerMinute.ShouldBe(500);
        r.MonthlyCallCredit.ShouldBe(10_000_000L);
    }
}
