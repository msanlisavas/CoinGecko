using CoinGecko.Api;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace CoinGecko.Api.MockTests.Infra;

/// <summary>
/// Fixture for mock tests that hit endpoints gated behind any paid plan. Uses <see cref="CoinGeckoPlan.Pro"/>
/// so every <see cref="RequiresPlanAttribute"/>-marked endpoint passes the plan handler.
/// </summary>
public sealed class CoinGeckoPaidMockFixture : IAsyncLifetime
{
    public WireMockServer Server { get; private set; } = default!;
    public ICoinGeckoClient Client { get; private set; } = default!;
    private ServiceProvider _sp = default!;

    public ValueTask InitializeAsync()
    {
        Server = WireMockServer.Start();

        var services = new ServiceCollection();
        services.AddCoinGeckoApi(opts =>
        {
            opts.ApiKey = "test-pro-key";
            opts.Plan = CoinGeckoPlan.Pro;
            opts.BaseAddress = new Uri(Server.Url! + "/api/v3/");
        });
        _sp = services.BuildServiceProvider();
        Client = _sp.GetRequiredService<ICoinGeckoClient>();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _sp.Dispose();
        Server.Stop();
        Server.Dispose();
        return ValueTask.CompletedTask;
    }
}
