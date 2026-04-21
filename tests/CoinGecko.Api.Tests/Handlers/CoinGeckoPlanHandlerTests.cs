using System.Net;
using CoinGecko.Api;
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Serialization;
using CoinGecko.Api.Tests.Infra;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.Tests.Handlers;

public class CoinGeckoPlanHandlerTests
{
    private static HttpClient Build(CoinGeckoOptions opts, out StubHandler inner)
    {
        inner = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var h = new CoinGeckoPlanHandler(new OptionsWrapper<CoinGeckoOptions>(opts)) { InnerHandler = inner };
        return new HttpClient(h);
    }

    private static HttpRequestMessage Req(string path, CoinGeckoPlan? required)
    {
        var r = new HttpRequestMessage(HttpMethod.Get, "https://example.com" + path);
        if (required is not null)
        {
            r.Options.Set(CoinGeckoRequestOptions.RequiredPlan, required);
        }
        return r;
    }

    [Fact]
    public async Task Passes_through_when_no_plan_required()
    {
        using var c = Build(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo }, out var stub);
        var resp = await c.SendAsync(Req("/ping", required: null), TestContext.Current.CancellationToken);
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        stub.Received.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Passes_through_when_plan_meets_requirement()
    {
        using var c = Build(new CoinGeckoOptions { Plan = CoinGeckoPlan.Pro }, out var stub);
        var resp = await c.SendAsync(Req("/x", required: CoinGeckoPlan.Analyst), TestContext.Current.CancellationToken);
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Throws_when_plan_below_requirement()
    {
        using var c = Build(new CoinGeckoOptions { Plan = CoinGeckoPlan.Demo }, out var stub);
        var ex = await Should.ThrowAsync<CoinGeckoPlanException>(
            () => c.SendAsync(Req("/x", required: CoinGeckoPlan.Analyst), TestContext.Current.CancellationToken));
        ex.RequiredPlan.ShouldBe(CoinGeckoPlan.Analyst);
        ex.ActualPlan.ShouldBe(CoinGeckoPlan.Demo);
        stub.Received.Count.ShouldBe(0, "the request must not have been sent downstream");
    }
}
