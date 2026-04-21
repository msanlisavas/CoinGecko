using Microsoft.Extensions.DependencyInjection;

namespace CoinGecko.Api;

/// <summary>Wraps a <see cref="ServiceProvider"/> and the resolved <see cref="ICoinGeckoClient"/>, providing deterministic disposal of the DI container.</summary>
public sealed class CoinGeckoClientScope : IDisposable
{
    private readonly ServiceProvider _sp;

    /// <summary>Gets the resolved <see cref="ICoinGeckoClient"/> instance backed by this scope.</summary>
    public ICoinGeckoClient Client { get; }

    internal CoinGeckoClientScope(ServiceProvider sp, ICoinGeckoClient client)
    {
        _sp = sp;
        Client = client;
    }

    /// <summary>Disposes the underlying <see cref="ServiceProvider"/> and all owned services.</summary>
    public void Dispose() => _sp.Dispose();
}

/// <summary>Provides a static factory for creating an <see cref="ICoinGeckoClient"/> without a host, suitable for scripts, console apps, and unit tests.</summary>
public static class CoinGeckoClientFactory
{
    /// <summary>Creates a self-contained <see cref="CoinGeckoClientScope"/> backed by an inline <see cref="ServiceProvider"/>.</summary>
    /// <param name="apiKey">The CoinGecko API key.</param>
    /// <param name="plan">The subscription tier; defaults to <see cref="CoinGeckoPlan.Demo"/>.</param>
    /// <param name="customize">Optional callback to further configure <see cref="CoinGeckoOptions"/>.</param>
    /// <returns>A <see cref="CoinGeckoClientScope"/> that owns the <see cref="ServiceProvider"/>; dispose it when done.</returns>
    public static CoinGeckoClientScope Create(string apiKey, CoinGeckoPlan plan = CoinGeckoPlan.Demo, Action<CoinGeckoOptions>? customize = null)
    {
        var services = new ServiceCollection();
        services.AddCoinGeckoApi(opts =>
        {
            opts.ApiKey = apiKey;
            opts.Plan = plan;
            customize?.Invoke(opts);
        });

        var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<ICoinGeckoClient>();
        return new CoinGeckoClientScope(sp, client);
    }
}
