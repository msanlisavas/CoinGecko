using System.Globalization;
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub.Projections;
using CoinGecko.Api.Models;
using CoinGecko.Api.Models.Onchain;

namespace CoinGecko.Api.AiAgentHub;

/// <summary>CoinGecko tool implementations for <see cref="Microsoft.Extensions.AI.AIFunctionFactory"/>.</summary>
public static class CoinGeckoTools
{
    /// <summary>Get current prices for one or more coins in a quote currency. Returns an array of price quotes.</summary>
    /// <param name="gecko">CoinGecko client (do not specify; provided by the host).</param>
    /// <param name="coinIds">CoinGecko coin ids (e.g. <c>["bitcoin","ethereum"]</c>).</param>
    /// <param name="vsCurrency">Quote currency (e.g. <c>"usd"</c>, <c>"eur"</c>).</param>
    public static async Task<CoinPriceQuote[]> GetCoinPrices(
        ICoinGeckoClient gecko,
        IReadOnlyList<string> coinIds,
        string vsCurrency = "usd")
    {
        var prices = await gecko.Simple.GetPriceAsync(new SimplePriceOptions
        {
            Ids = coinIds,
            VsCurrencies = new[] { vsCurrency },
            Include24hrChange = true,
            IncludeMarketCap = true,
        });

        var result = new List<CoinPriceQuote>(coinIds.Count);
        foreach (var id in coinIds)
        {
            if (!prices.TryGetValue(id, out var inner))
            {
                continue;
            }

            if (!inner.TryGetValue(vsCurrency, out var price) || price is null)
            {
                continue;
            }

            inner.TryGetValue($"{vsCurrency}_24h_change", out var change);
            inner.TryGetValue($"{vsCurrency}_market_cap", out var mcap);
            result.Add(new CoinPriceQuote(
                CoinId: id, Symbol: id.ToUpperInvariant(), Name: id, VsCurrency: vsCurrency,
                Price: price.Value, Change24hPercent: change, MarketCap: mcap));
        }
        return result.ToArray();
    }

    /// <summary>Search CoinGecko for coins, exchanges, categories, or NFTs by name/symbol/ticker.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="query">Search terms.</param>
    /// <param name="maxResults">Cap on returned hits.</param>
    public static async Task<CoinSearchHit[]> Search(
        ICoinGeckoClient gecko, string query, int maxResults = 10)
    {
        var r = await gecko.Search.SearchAsync(query);
        var hits = new List<CoinSearchHit>();

        foreach (var c in r.Coins.Take(maxResults))
        {
            hits.Add(new CoinSearchHit("coin", c.Id ?? "", c.Symbol ?? "", c.Name ?? "", c.MarketCapRank));
        }

        foreach (var n in r.Nfts.Take(maxResults - hits.Count))
        {
            if (hits.Count >= maxResults)
            {
                break;
            }

            hits.Add(new CoinSearchHit("nft", n.Id ?? "", n.Symbol ?? "", n.Name ?? "", Rank: null));
        }

        foreach (var e in r.Exchanges.Take(maxResults - hits.Count))
        {
            if (hits.Count >= maxResults)
            {
                break;
            }

            hits.Add(new CoinSearchHit("exchange", e.Id ?? "", "", e.Name ?? "", Rank: null));
        }

        foreach (var cat in r.Categories.Take(maxResults - hits.Count))
        {
            if (hits.Count >= maxResults)
            {
                break;
            }

            hits.Add(new CoinSearchHit("category", cat.Id?.ToString(CultureInfo.InvariantCulture) ?? "", "", cat.Name ?? "", Rank: null));
        }

        return hits.ToArray();
    }

    /// <summary>Get top coins by market cap in a given quote currency.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="vsCurrency">Quote currency (e.g. <c>"usd"</c>).</param>
    /// <param name="limit">Number of rows (1–100).</param>
    public static async Task<MarketSnapshot[]> GetTopMarkets(
        ICoinGeckoClient gecko, string vsCurrency = "usd", int limit = 25)
    {
        var rows = await gecko.Coins.GetMarketsAsync(vsCurrency,
            new CoinMarketsOptions { PerPage = limit, Page = 1 });
        return rows.Select(m => new MarketSnapshot(
            Rank: m.MarketCapRank,
            CoinId: m.Id ?? "",
            Symbol: (m.Symbol ?? "").ToUpperInvariant(),
            Name: m.Name ?? "",
            Price: m.CurrentPrice,
            Change24hPercent: m.PriceChangePercentage24h,
            MarketCap: m.MarketCap,
            Volume24h: m.TotalVolume)).Take(limit).ToArray();
    }

    /// <summary>Get currently trending coins, NFTs, and categories.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="maxItems">Cap on items per section.</param>
    public static async Task<TrendingSummary> GetTrending(
        ICoinGeckoClient gecko, int maxItems = 10)
    {
        var r = await gecko.Trending.GetAsync();
        return new TrendingSummary(
            Coins: r.Coins.Take(maxItems)
                .Select(c => c.Item is null
                    ? null
                    : new TrendingCoinSummary(c.Item.Id ?? "", c.Item.Symbol ?? "", c.Item.Name ?? "", c.Item.MarketCapRank))
                .OfType<TrendingCoinSummary>()
                .ToArray(),
            Nfts: r.Nfts.Take(maxItems)
                .Select(n => new TrendingNftSummary(n.Id ?? "", n.Name ?? "", n.Symbol ?? "", n.FloorPrice24hPercentageChange))
                .ToArray(),
            Categories: r.Categories.Take(maxItems)
                .Select(c => new TrendingCategorySummary(
                    c.Id?.ToString(CultureInfo.InvariantCulture) ?? "", c.Name ?? "", c.MarketCap1hChange))
                .ToArray());
    }

    /// <summary>List coin categories with market data.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="limit">Cap on rows.</param>
    public static async Task<CategorySummary[]> GetCategories(
        ICoinGeckoClient gecko, int limit = 25)
    {
        var rows = await gecko.Categories.GetAsync();
        return rows.Take(limit).Select(c => new CategorySummary(
            Id: c.Id ?? "", Name: c.Name ?? "",
            MarketCapUsd: c.MarketCap, Volume24hUsd: c.Volume24h,
            Change24hPercent: c.MarketCapChange24h)).ToArray();
    }

    /// <summary>Get NFT collection detail by id.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="collectionId">NFT collection id.</param>
    public static async Task<NftSummary> GetNft(
        ICoinGeckoClient gecko, string collectionId)
    {
        var n = await gecko.Nfts.GetAsync(collectionId);
        return new NftSummary(
            Id: n.Id ?? "", Name: n.Name ?? "", Symbol: n.Symbol ?? "",
            AssetPlatformId: n.AssetPlatformId,
            FloorPriceNative: n.FloorPrice is not null && n.FloorPrice.TryGetValue("native_currency", out var f) ? f : null,
            FloorPriceUsd: n.FloorPrice is not null && n.FloorPrice.TryGetValue("usd", out var fu) ? fu : null,
            MarketCapUsd: n.MarketCap is not null && n.MarketCap.TryGetValue("usd", out var mu) ? mu : null,
            Holders: n.NumberOfUniqueAddresses);
    }

    /// <summary>List derivative tickers.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="limit">Cap on rows.</param>
    public static async Task<DerivativeSummary[]> GetDerivatives(
        ICoinGeckoClient gecko, int limit = 25)
    {
        var rows = await gecko.Derivatives.GetTickersAsync();
        return rows.Take(limit).Select(d => new DerivativeSummary(
            Market: d.Market ?? "", Symbol: d.Symbol ?? "",
            Price: d.Price, Change24hPercent: d.PricePercentageChange24h,
            FundingRate: d.FundingRate, Volume24hUsd: d.Volume24h)).ToArray();
    }

    /// <summary>Search on-chain pools by name / symbol / contract.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="query">Search terms.</param>
    /// <param name="network">Filter to one network (optional).</param>
    /// <param name="limit">Cap on rows.</param>
    public static async Task<OnchainPoolSummary[]> SearchOnchainPools(
        ICoinGeckoClient gecko, string query, string? network = null, int limit = 20)
    {
        var pools = await gecko.Onchain.SearchPoolsAsync(query, network);
        return pools.Take(limit).Select(p => new OnchainPoolSummary(
            Id: p.Id ?? "",
            Name: p.Attributes?.Name ?? "",
            Address: p.Attributes?.Address ?? "",
            NetworkId: null,
            BaseTokenPriceUsd: p.Attributes?.BaseTokenPriceUsd,
            QuoteTokenPriceUsd: p.Attributes?.QuoteTokenPriceUsd,
            ReserveUsd: p.Attributes?.ReserveInUsd,
            Volume24hUsd: p.Attributes?.VolumeUsd?.TryGetValue("h24", out var v24) == true ? v24 : null)).ToArray();
    }

    /// <summary>Get on-chain token prices by contract address.</summary>
    /// <param name="gecko">CoinGecko client.</param>
    /// <param name="network">Network id (e.g. <c>"eth"</c>).</param>
    /// <param name="contractAddresses">Contract addresses.</param>
    public static async Task<OnchainTokenPriceQuote[]> GetOnchainTokenPrices(
        ICoinGeckoClient gecko, string network, IReadOnlyList<string> contractAddresses)
    {
        var r = await gecko.Onchain.GetTokenPriceAsync(network, contractAddresses, options: null);
        var prices = r.Attributes?.TokenPrices;
        if (prices is null)
        {
            return Array.Empty<OnchainTokenPriceQuote>();
        }

        return prices
            .Where(kvp => kvp.Value.HasValue)
            .Select(kvp => new OnchainTokenPriceQuote(
                NetworkId: network, Address: kvp.Key, PriceUsd: kvp.Value!.Value))
            .ToArray();
    }
}
