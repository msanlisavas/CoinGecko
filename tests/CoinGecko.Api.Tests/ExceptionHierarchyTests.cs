using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Exceptions;

namespace CoinGecko.Api.Tests;

public class ExceptionHierarchyTests
{
    [Theory]
    [InlineData(typeof(CoinGeckoRateLimitException))]
    [InlineData(typeof(CoinGeckoPlanException))]
    [InlineData(typeof(CoinGeckoAuthException))]
    [InlineData(typeof(CoinGeckoNotFoundException))]
    [InlineData(typeof(CoinGeckoValidationException))]
    [InlineData(typeof(CoinGeckoServerException))]
    public void All_derived_types_inherit_CoinGeckoException(Type type)
    {
        type.IsSubclassOf(typeof(CoinGeckoException)).ShouldBeTrue();
    }

    [Fact]
    public void Base_exposes_status_and_raw_body()
    {
        var ex = new CoinGeckoAuthException(HttpStatusCode.Unauthorized, "{\"error\":\"bad key\"}", requestId: Guid.Empty);
        ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        ex.RawBody.ShouldBe("{\"error\":\"bad key\"}");
        ex.RequestId.ShouldBe(Guid.Empty);
        ex.Message.ShouldContain("401");
    }

    [Fact]
    public void RateLimit_exposes_retry_after()
    {
        var ex = new CoinGeckoRateLimitException(TimeSpan.FromSeconds(7), rawBody: "", requestId: Guid.NewGuid());
        ex.RetryAfter.ShouldBe(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void Plan_exposes_required_plan()
    {
        var ex = new CoinGeckoPlanException(required: CoinGeckoPlan.Analyst, actual: CoinGeckoPlan.Demo);
        ex.RequiredPlan.ShouldBe(CoinGeckoPlan.Analyst);
        ex.ActualPlan.ShouldBe(CoinGeckoPlan.Demo);
    }
}
