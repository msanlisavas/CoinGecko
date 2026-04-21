using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.AI;

namespace CoinGecko.Api.AiAgentHub;

/// <summary>Factory that produces <see cref="AIFunction"/> tools for an <see cref="CoinGecko.Api.ICoinGeckoClient"/>.</summary>
public static class CoinGeckoAiTools
{
    /// <summary>
    /// Build an array of <see cref="AIFunction"/> tools bound to the given <see cref="CoinGecko.Api.ICoinGeckoClient"/>.
    /// Pass the result as <c>ChatOptions.Tools</c> to any <see cref="IChatClient"/>.
    /// </summary>
    /// <param name="client">The underlying REST client.</param>
    /// <param name="options">Filtering and safety options.</param>
    [RequiresUnreferencedCode("AIFunctionFactory.Create uses reflection over method metadata. Not trim-safe. Use v0.2+ source-gen alternative for AOT scenarios.")]
    [RequiresDynamicCode("AIFunctionFactory.Create uses reflection. Not AOT-compatible.")]
    public static IReadOnlyList<AIFunction> Create(
        CoinGecko.Api.ICoinGeckoClient client,
        CoinGeckoAiToolsOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        return Array.Empty<AIFunction>();
    }
}
