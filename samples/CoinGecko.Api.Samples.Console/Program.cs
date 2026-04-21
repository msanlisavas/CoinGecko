using CoinGecko.Api;
using CoinGecko.Api.Exceptions;
using CoinGecko.Api.Models;
using CoinGecko.Api.Models.Onchain;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY");
    opts.Plan = CoinGeckoPlan.Demo;
});

using var sp = services.BuildServiceProvider();
var gecko = sp.GetRequiredService<ICoinGeckoClient>();

// Smoke-test runner: (label, action). Each catches its own exceptions so one failure doesn't abort.
var checks = new (string Label, Func<Task> Run)[]
{
    // ─── Ping ───
    ("Ping.PingAsync", async () =>
    {
        var r = await gecko.Ping.PingAsync();
        Console.WriteLine($"    → {r.GeckoSays}");
    }),

    // ─── Simple ───
    ("Simple.GetPriceAsync", async () =>
    {
        var r = await gecko.Simple.GetPriceAsync(new SimplePriceOptions
        {
            Ids = ["bitcoin", "ethereum"],
            VsCurrencies = ["usd", "eur"],
            IncludeMarketCap = true,
            Include24hrChange = true,
            Include24hrVol = true,
            IncludeLastUpdatedAt = true,
        });
        Console.WriteLine($"    → {r.Count} coins, BTC usd=${r["bitcoin"]["usd"]:N2}");
    }),
    ("Simple.GetTokenPriceAsync(eth/usdc)", async () =>
    {
        // USDC on Ethereum
        var r = await gecko.Simple.GetTokenPriceAsync("ethereum", new SimpleTokenPriceOptions
        {
            ContractAddresses = ["0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48"],
            VsCurrencies = ["usd"],
        });
        Console.WriteLine($"    → {r.Count} tokens");
    }),
    ("Simple.GetSupportedVsCurrencies", async () =>
    {
        var r = await gecko.Simple.GetSupportedVsCurrenciesAsync();
        Console.WriteLine($"    → {r.Count} currencies");
    }),

    // ─── Coins ───
    ("Coins.GetListAsync", async () =>
    {
        var r = await gecko.Coins.GetListAsync();
        Console.WriteLine($"    → {r.Count} coins");
    }),
    ("Coins.GetMarketsAsync", async () =>
    {
        var r = await gecko.Coins.GetMarketsAsync("usd", new CoinMarketsOptions { PerPage = 10 });
        Console.WriteLine($"    → {r.Count} rows, #1: {r[0].Name} ${r[0].CurrentPrice:N2}");
    }),
    ("Coins.GetAsync(bitcoin)", async () =>
    {
        var r = await gecko.Coins.GetAsync("bitcoin");
        Console.WriteLine($"    → {r.Name} rank {r.MarketCapRank}");
    }),
    ("Coins.GetTickersAsync(bitcoin)", async () =>
    {
        var r = await gecko.Coins.GetTickersAsync("bitcoin");
        Console.WriteLine($"    → {r.Tickers.Count} tickers, first: {r.Tickers[0].Market?.Name} {r.Tickers[0].Base}/{r.Tickers[0].Target}");
    }),
    ("Coins.GetHistoryAsync(BTC, 30d ago)", async () =>
    {
        // Demo plan only allows last 365 days; use a date 30 days ago to be safely inside the window.
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var r = await gecko.Coins.GetHistoryAsync("bitcoin", date);
        Console.WriteLine($"    → {r.Name} on {date:yyyy-MM-dd}");
    }),
    ("Coins.GetMarketChartAsync(BTC,7d)", async () =>
    {
        var r = await gecko.Coins.GetMarketChartAsync("bitcoin", "usd", MarketChartRange.SevenDays);
        Console.WriteLine($"    → {r.Prices.Count} price points, {r.MarketCaps.Count} mcap, {r.TotalVolumes.Count} volume");
    }),
    ("Coins.GetMarketChartRangeAsync(BTC)", async () =>
    {
        var from = DateTimeOffset.UtcNow.AddDays(-7);
        var to = DateTimeOffset.UtcNow;
        var r = await gecko.Coins.GetMarketChartRangeAsync("bitcoin", "usd", from, to);
        Console.WriteLine($"    → {r.Prices.Count} price points over 7d");
    }),
    ("Coins.GetOhlcAsync(BTC,1d)", async () =>
    {
        var r = await gecko.Coins.GetOhlcAsync("bitcoin", "usd", MarketChartRange.OneDay);
        Console.WriteLine($"    → {r.Count} candles");
    }),
    ("Coins.GetByContractAsync(eth/usdc)", async () =>
    {
        var r = await gecko.Coins.GetByContractAsync("ethereum", "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48");
        Console.WriteLine($"    → {r.Name} ({r.Symbol})");
    }),
    ("Coins.GetContractMarketChartAsync(eth/usdc,7d)", async () =>
    {
        var r = await gecko.Coins.GetContractMarketChartAsync("ethereum", "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48", "usd", MarketChartRange.SevenDays);
        Console.WriteLine($"    → {r.Prices.Count} price points");
    }),

    // ─── Nfts ───
    ("Nfts.GetListAsync(10)", async () =>
    {
        var r = await gecko.Nfts.GetListAsync(10, 1);
        Console.WriteLine($"    → {r.Count} NFTs");
    }),
    ("Nfts.GetAsync(autoglyphs)", async () =>
    {
        var r = await gecko.Nfts.GetAsync("autoglyphs");
        Console.WriteLine($"    → {r.Name}, {r.NumberOfUniqueAddresses} holders");
    }),
    ("Nfts.GetByContractAsync(eth/autoglyphs)", async () =>
    {
        var r = await gecko.Nfts.GetByContractAsync("ethereum", "0xd4e4078ca3495de5b1d4db434bebc5a986197782");
        Console.WriteLine($"    → {r.Name}");
    }),

    // ─── Exchanges ───
    ("Exchanges.GetAsync(top 5)", async () =>
    {
        var r = await gecko.Exchanges.GetAsync(new ExchangesOptions { PerPage = 5 });
        Console.WriteLine($"    → {r.Count}, top: {r[0].Name} trust={r[0].TrustScore}");
    }),
    ("Exchanges.GetListAsync", async () =>
    {
        var r = await gecko.Exchanges.GetListAsync();
        Console.WriteLine($"    → {r.Count} exchanges");
    }),
    ("Exchanges.GetByIdAsync(binance)", async () =>
    {
        var r = await gecko.Exchanges.GetByIdAsync("binance");
        Console.WriteLine($"    → {r.Name}, year {r.YearEstablished}, {r.Tickers?.Count ?? 0} embedded tickers");
    }),
    ("Exchanges.GetTickersAsync(binance)", async () =>
    {
        var r = await gecko.Exchanges.GetTickersAsync("binance");
        Console.WriteLine($"    → {r.Name}: {r.Tickers.Count} tickers");
    }),
    ("Exchanges.GetVolumeChartAsync(binance,7)", async () =>
    {
        var r = await gecko.Exchanges.GetVolumeChartAsync("binance", 7);
        Console.WriteLine($"    → {r.Count} daily-volume points");
    }),

    // ─── Derivatives ───
    ("Derivatives.GetTickersAsync", async () =>
    {
        var r = await gecko.Derivatives.GetTickersAsync();
        Console.WriteLine($"    → {r.Count} tickers, first: {r[0].Market} {r[0].Symbol}");
    }),
    ("Derivatives.GetExchangesAsync(5)", async () =>
    {
        var r = await gecko.Derivatives.GetExchangesAsync(new DerivativeExchangesOptions { PerPage = 5 });
        Console.WriteLine($"    → {r.Count} exchanges, top: {r[0].Name}");
    }),
    ("Derivatives.GetExchangeAsync(binance_futures)", async () =>
    {
        var r = await gecko.Derivatives.GetExchangeAsync("binance_futures", includeTickers: false);
        Console.WriteLine($"    → {r.Name}, OI(btc) {r.OpenInterestBtc:N2}");
    }),
    ("Derivatives.GetExchangeListAsync", async () =>
    {
        var r = await gecko.Derivatives.GetExchangeListAsync();
        Console.WriteLine($"    → {r.Count} exchange ids");
    }),

    // ─── Categories ───
    ("Categories.GetListAsync", async () =>
    {
        var r = await gecko.Categories.GetListAsync();
        Console.WriteLine($"    → {r.Count} categories");
    }),
    ("Categories.GetAsync", async () =>
    {
        var r = await gecko.Categories.GetAsync();
        Console.WriteLine($"    → {r.Count} with market data, top: {r[0].Name}");
    }),

    // ─── AssetPlatforms ───
    ("AssetPlatforms.GetListAsync", async () =>
    {
        var r = await gecko.AssetPlatforms.GetListAsync();
        Console.WriteLine($"    → {r.Count} platforms");
    }),

    // ─── Simple ───
    // (already above)

    // ─── Global ───
    ("Global.GetAsync", async () =>
    {
        var r = await gecko.Global.GetAsync();
        Console.WriteLine($"    → {r.ActiveCryptocurrencies} coins, {r.Markets} markets");
    }),
    ("Global.GetDefiAsync", async () =>
    {
        var r = await gecko.Global.GetDefiAsync();
        Console.WriteLine($"    → DeFi mcap ${r.DefiMarketCap}");
    }),

    // ─── Search ───
    ("Search.SearchAsync(ethereum)", async () =>
    {
        var r = await gecko.Search.SearchAsync("ethereum");
        Console.WriteLine($"    → {r.Coins.Count}c/{r.Exchanges.Count}x/{r.Nfts.Count}n/{r.Categories.Count}cat");
    }),

    // ─── Trending ───
    ("Trending.GetAsync", async () =>
    {
        var r = await gecko.Trending.GetAsync();
        Console.WriteLine($"    → {r.Coins.Count} coins / {r.Nfts.Count} nfts / {r.Categories.Count} cats");
    }),

    // ─── Onchain (JSON:API) ───
    ("Onchain.GetNetworksAsync", async () =>
    {
        var r = await gecko.Onchain.GetNetworksAsync();
        Console.WriteLine($"    → {r.Length} networks");
    }),
    ("Onchain.GetDexesAsync(eth)", async () =>
    {
        var r = await gecko.Onchain.GetDexesAsync("eth");
        Console.WriteLine($"    → {r.Length} dexes on eth");
    }),
    ("Onchain.GetTrendingPoolsAsync", async () =>
    {
        var r = await gecko.Onchain.GetTrendingPoolsAsync(new OnchainTrendingPoolsOptions { Include = ["base_token","quote_token"] });
        Console.WriteLine($"    → {r.Length} pools");
    }),
    ("Onchain.GetTrendingPoolsByNetworkAsync(eth)", async () =>
    {
        var r = await gecko.Onchain.GetTrendingPoolsByNetworkAsync("eth", new OnchainTrendingPoolsOptions { Include = ["base_token"] });
        Console.WriteLine($"    → {r.Length} trending pools on eth");
    }),
    ("Onchain.GetTopPoolsByNetworkAsync(eth)", async () =>
    {
        var r = await gecko.Onchain.GetTopPoolsByNetworkAsync("eth", new OnchainPoolsListOptions());
        Console.WriteLine($"    → {r.Length} top pools on eth");
    }),
    ("Onchain.GetNewPoolsAsync", async () =>
    {
        var r = await gecko.Onchain.GetNewPoolsAsync(new OnchainPoolsListOptions());
        Console.WriteLine($"    → {r.Length} new pools");
    }),
    ("Onchain.GetNewPoolsByNetworkAsync(eth)", async () =>
    {
        var r = await gecko.Onchain.GetNewPoolsByNetworkAsync("eth", new OnchainPoolsListOptions());
        Console.WriteLine($"    → {r.Length} new pools on eth");
    }),
    ("Onchain.SearchPoolsAsync(doge)", async () =>
    {
        var r = await gecko.Onchain.SearchPoolsAsync("doge");
        Console.WriteLine($"    → {r.Length} pool hits");
    }),

    // ─── Plan gating: call Analyst+ endpoint with Demo, expect CoinGeckoPlanException ───
    ("[plan-gate] GetOhlcRangeAsync (Analyst+) → PlanException", async () =>
    {
        try
        {
            await gecko.Coins.GetOhlcRangeAsync("bitcoin", "usd",
                DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);
            throw new InvalidOperationException("expected CoinGeckoPlanException but call succeeded");
        }
        catch (CoinGeckoPlanException ex)
        {
            Console.WriteLine($"    → correctly threw: required={ex.RequiredPlan}, actual={ex.ActualPlan}");
        }
    }),
    ("[plan-gate] TopGainersLosersAsync (Analyst+) → PlanException", async () =>
    {
        try
        {
            await gecko.Coins.GetTopGainersLosersAsync("usd");
            throw new InvalidOperationException("expected CoinGeckoPlanException but call succeeded");
        }
        catch (CoinGeckoPlanException ex)
        {
            Console.WriteLine($"    → correctly threw: required={ex.RequiredPlan}");
        }
    }),

    // ─── Error handling: unknown coin id → NotFoundException ───
    ("[error] Coins.GetAsync(nonexistent) → NotFoundException", async () =>
    {
        try
        {
            await gecko.Coins.GetAsync("this-coin-does-not-exist-xyz123");
            throw new InvalidOperationException("expected CoinGeckoNotFoundException but call succeeded");
        }
        catch (CoinGeckoNotFoundException)
        {
            Console.WriteLine("    → correctly threw CoinGeckoNotFoundException");
        }
    }),
};

Console.WriteLine($"Running {checks.Length} smoke-test calls against Demo plan...\n");

var passed = 0;
var failed = 0;
var sw = System.Diagnostics.Stopwatch.StartNew();

foreach (var (label, run) in checks)
{
    var start = sw.Elapsed;
    Console.Write($"• {label,-52}");
    try
    {
        await run();
        Console.WriteLine($"    ({(sw.Elapsed - start).TotalMilliseconds:N0} ms)");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"    FAILED: {ex.GetType().Name}: {ex.Message}");
        failed++;
    }
}

Console.WriteLine();
Console.WriteLine($"=== {passed}/{checks.Length} passed, {failed} failed — {sw.Elapsed.TotalSeconds:N1}s total ===");
return failed == 0 ? 0 : 1;
