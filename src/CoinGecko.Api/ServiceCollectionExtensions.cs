using CoinGecko.Api.Handlers;
using CoinGecko.Api.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace CoinGecko.Api;

/// <summary>Extension methods on <see cref="IServiceCollection"/> for registering CoinGecko API services.</summary>
public static class ServiceCollectionExtensions
{
    private const string HttpClientName = "CoinGecko.Api";

    /// <summary>Registers <see cref="ICoinGeckoClient"/> and all sub-clients. Returns the <see cref="IHttpClientBuilder"/> so callers can chain additional handler configuration.</summary>
    public static IHttpClientBuilder AddCoinGeckoApi(this IServiceCollection services, Action<CoinGeckoOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<CoinGeckoOptions>();
        }

        services.AddTransient<CoinGeckoAuthHandler>();
        services.AddTransient<CoinGeckoPlanHandler>();
        services.AddTransient<CoinGeckoRateLimitHandler>();
        services.AddTransient<CoinGeckoRetryHandler>();

        var builder = services.AddHttpClient(HttpClientName, (sp, c) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CoinGeckoOptions>>().Value;
            c.BaseAddress = opts.BaseAddress ?? new Uri(opts.Plan == CoinGeckoPlan.Demo
                ? "https://api.coingecko.com/api/v3/"
                : "https://pro-api.coingecko.com/api/v3/");
        })
        .AddHttpMessageHandler<CoinGeckoAuthHandler>()
        .AddHttpMessageHandler<CoinGeckoPlanHandler>()
        .AddHttpMessageHandler<CoinGeckoRateLimitHandler>()
        .AddHttpMessageHandler<CoinGeckoRetryHandler>();

        services.AddTransient<IPingClient>(sp => new PingClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ICoinsClient>(sp => new CoinsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<INftsClient>(sp => new NftsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IExchangesClient>(sp => new ExchangesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IDerivativesClient>(sp => new DerivativesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ICategoriesClient>(sp => new CategoriesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IAssetPlatformsClient>(sp => new AssetPlatformsClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ICompaniesClient>(sp => new CompaniesClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ISimpleClient>(sp => new SimpleClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IGlobalClient>(sp => new GlobalClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ISearchClient>(sp => new SearchClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<ITrendingClient>(sp => new TrendingClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IOnchainClient>(sp => new OnchainClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        services.AddTransient<IKeyClient>(sp => new KeyClient(sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));

        services.AddTransient<ICoinGeckoClient, CoinGeckoClient>();

        return builder;
    }
}
