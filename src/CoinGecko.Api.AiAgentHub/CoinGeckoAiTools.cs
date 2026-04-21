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
        var opts = options ?? new CoinGeckoAiToolsOptions();

        var tools = new List<AIFunction>();
        var filter = opts.ToolFilter;

        void Add(CoinGeckoToolSet flag, Delegate del, string name, string description)
        {
            if ((opts.Tools & flag) == 0)
            {
                return;
            }

            if (filter is not null && !filter(name))
            {
                return;
            }

            var fn = AIFunctionFactory.Create(
                del,
                new AIFunctionFactoryOptions { Name = name, Description = description });
            tools.Add(fn);
        }

        // CoinPrices
        Add(CoinGeckoToolSet.CoinPrices,
            (IReadOnlyList<string> coinIds, string vsCurrency)
                => CoinGeckoTools.GetCoinPrices(client, coinIds, vsCurrency),
            name: "get_coin_prices",
            description: "Get current prices for one or more coins in a quote currency. Useful when you know coin ids like \"bitcoin\" or \"ethereum\" and want the live price.");

        // CoinSearch
        Add(CoinGeckoToolSet.CoinSearch,
            (string query, int maxResults)
                => CoinGeckoTools.Search(client, query, maxResults),
            name: "coin_search",
            description: "Search CoinGecko for coins, NFTs, exchanges, and categories by name or ticker. Use this first when the user mentions a coin by ticker or partial name.");

        // MarketData
        Add(CoinGeckoToolSet.MarketData,
            (string vsCurrency, int limit)
                => CoinGeckoTools.GetTopMarkets(client, vsCurrency, limit),
            name: "get_top_markets",
            description: "Get the top coins ranked by market capitalization. Use for overview-style questions about the market.");

        // Trending
        Add(CoinGeckoToolSet.Trending,
            (int maxItems)
                => CoinGeckoTools.GetTrending(client, maxItems),
            name: "get_trending",
            description: "Get currently trending coins, NFT collections, and categories. Use when the user asks \"what's hot\" / \"what's trending\".");

        // Categories
        Add(CoinGeckoToolSet.Categories,
            (int limit)
                => CoinGeckoTools.GetCategories(client, limit),
            name: "get_categories",
            description: "List coin sector categories (DeFi, gaming, L1, L2, etc.) with aggregate market data.");

        // Nfts
        Add(CoinGeckoToolSet.Nfts,
            (string collectionId)
                => CoinGeckoTools.GetNft(client, collectionId),
            name: "get_nft_collection",
            description: "Get detail for one NFT collection by its CoinGecko id.");

        // Derivatives
        Add(CoinGeckoToolSet.Derivatives,
            (int limit)
                => CoinGeckoTools.GetDerivatives(client, limit),
            name: "get_derivatives",
            description: "List current derivative tickers (futures, perpetuals).");

        // Onchain tools (gated by both flag AND IncludeOnchainTools)
        if (opts.IncludeOnchainTools)
        {
            Add(CoinGeckoToolSet.Onchain,
                (string query, string? network, int limit)
                    => CoinGeckoTools.SearchOnchainPools(client, query, network, limit),
                name: "search_onchain_pools",
                description: "Search DEX liquidity pools by name / symbol / contract. Filter by network id (e.g. \"eth\", \"bsc\") optionally.");

            Add(CoinGeckoToolSet.Onchain,
                (string network, IReadOnlyList<string> contractAddresses)
                    => CoinGeckoTools.GetOnchainTokenPrices(client, network, contractAddresses),
                name: "get_onchain_token_prices",
                description: "Get on-chain token prices in USD by contract address. Use for tokens not listed on CoinGecko's main aggregator.");
        }

        return tools;
    }
}
