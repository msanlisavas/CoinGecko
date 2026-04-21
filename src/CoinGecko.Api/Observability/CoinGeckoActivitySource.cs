using System.Diagnostics;
using System.Reflection;

namespace CoinGecko.Api.Observability;

internal static class CoinGeckoActivitySource
{
    public const string Name = "CoinGecko.Api";

    public static readonly ActivitySource Instance = new(
        Name,
        typeof(CoinGeckoActivitySource).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0");
}
