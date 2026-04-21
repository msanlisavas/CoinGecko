using System.Diagnostics;
using System.Net;
using CoinGecko.Api.Handlers;
using CoinGecko.Api.Observability;
using CoinGecko.Api.Tests.Infra;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoinGecko.Api.Tests.Observability;

public class TelemetryTests
{
    [Fact]
    public async Task Successful_request_emits_an_activity_with_http_tags()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = src => src.Name == "CoinGecko.Api",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        var captured = new List<Activity>();
        listener.ActivityStopped = captured.Add;

        var stub = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var telemetry = new CoinGeckoTelemetryHandler(NullLogger<CoinGeckoTelemetryHandler>.Instance)
        {
            InnerHandler = stub,
        };
        using var client = new HttpClient(telemetry) { BaseAddress = new Uri("https://example.com/") };

        var resp = await client.GetAsync("ping", TestContext.Current.CancellationToken);
        resp.StatusCode.ShouldBe(HttpStatusCode.OK);

        captured.Count.ShouldBe(1);
        captured[0].GetTagItem("http.method").ShouldBe("GET");
        captured[0].GetTagItem("http.status_code").ShouldBe(200);
    }
}
