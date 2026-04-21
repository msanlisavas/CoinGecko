using System.Net;
using System.Text;
using CoinGecko.Api.Resources;
using CoinGecko.Api.Tests.Infra;

namespace CoinGecko.Api.Tests.Resources;

public class CompaniesClientTests
{
    [Fact]
    public async Task GetPublicTreasuryAsync_hits_companies_public_treasury_and_deserializes()
    {
        var stub = new StubHandler((req, _) =>
        {
            req.Method.ShouldBe(HttpMethod.Get);
            req.RequestUri!.AbsolutePath.ShouldContain("companies/public_treasury/bitcoin");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {"total_holdings":580000.0,"total_value_usd":35000000000.0,"market_cap_dominance":2.9,"companies":[{"name":"MicroStrategy","symbol":"NASDAQ:MSTR","country":"US","total_holdings":214246.0,"total_entry_value_usd":7633000000.0,"total_current_value_usd":13200000000.0,"percentage_of_total_supply":1.02}]}
                    """, Encoding.UTF8, "application/json"),
            };
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://api.coingecko.com/api/v3/") };
        var sut = new CompaniesClient(http);

        var r = await sut.GetPublicTreasuryAsync("bitcoin", TestContext.Current.CancellationToken);
        r.TotalHoldings.ShouldBe(580000.0m);
        r.Companies.Count.ShouldBe(1);
        r.Companies[0].Name.ShouldBe("MicroStrategy");
    }
}
