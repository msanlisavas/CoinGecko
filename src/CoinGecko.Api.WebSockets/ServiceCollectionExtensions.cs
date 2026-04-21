using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoinGecko.Api.WebSockets;

/// <summary>DI extensions for <see cref="ICoinGeckoStream"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Register a singleton <see cref="ICoinGeckoStream"/> with the given options.</summary>
    public static IServiceCollection AddCoinGeckoStream(
        this IServiceCollection services, Action<CoinGeckoStreamOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<CoinGeckoStreamOptions>();
        }

        services.AddSingleton<ICoinGeckoStream>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CoinGeckoStreamOptions>>().Value;
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<CoinGeckoStream>>();
            return new CoinGeckoStream(opts, logger);
        });

        return services;
    }
}
