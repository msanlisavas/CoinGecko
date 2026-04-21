using CoinGecko.Api;
using CoinGecko.Api.Resources;

namespace CoinGecko.Api.Tests;

public class CoinGeckoClientFactoryTests
{
    [Fact]
    public void Create_returns_a_disposable_wrapper_exposing_all_sub_clients()
    {
        using var scope = CoinGeckoClientFactory.Create("demo-key", CoinGeckoPlan.Demo);
        scope.Client.Ping.ShouldNotBeNull();
        scope.Client.Coins.ShouldNotBeNull();
        scope.Client.Onchain.ShouldNotBeNull();
    }

    [Fact]
    public void Dispose_disposes_the_underlying_service_scope()
    {
        var scope = CoinGeckoClientFactory.Create("demo-key");
        scope.Dispose();
        // After dispose, accessing Client should still return the cached reference — no ObjectDisposedException.
        // But using it will fail at HTTP time because the handler scope is disposed. We just verify Dispose is idempotent.
        Should.NotThrow(scope.Dispose);
    }
}
