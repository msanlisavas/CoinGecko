using CoinGecko.Api;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace CoinGecko.Api.MockTests.Infra;

public sealed class CoinGeckoMockFixture : IAsyncLifetime
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
            opts.ApiKey = "test-demo-key";
            opts.Plan = CoinGeckoPlan.Demo;
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
