using CoinGecko.Api;
using CoinGecko.Api.Models;
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
    ("Ping", async () =>
    {
        var r = await gecko.Ping.PingAsync();
        Console.WriteLine($"    → {r.GeckoSays}");
    }),

    ("Simple.GetPrice(BTC,ETH/USD)", async () =>
    {
        var r = await gecko.Simple.GetPriceAsync(new SimplePriceOptions
        {
            Ids = ["bitcoin", "ethereum"],
            VsCurrencies = ["usd"],
            IncludeMarketCap = true,
            Include24hrChange = true,
        });
        foreach (var kvp in r)
        {
            var usd = kvp.Value.TryGetValue("usd", out var v1) ? v1 : null;
            var chg = kvp.Value.TryGetValue("usd_24h_change", out var v2) ? v2 : null;
            Console.WriteLine($"    → {kvp.Key,-10} ${usd,10:N2}  24h: {chg:+0.00;-0.00}%");
        }
    }),

    ("Simple.GetSupportedVsCurrencies", async () =>
    {
        var r = await gecko.Simple.GetSupportedVsCurrenciesAsync();
        Console.WriteLine($"    → {r.Count} currencies, first 5: {string.Join(", ", r.Take(5))}");
    }),

    ("Coins.GetList", async () =>
    {
        var r = await gecko.Coins.GetListAsync();
        Console.WriteLine($"    → {r.Count} coins, first: {r[0].Id} / {r[0].Symbol} / {r[0].Name}");
    }),

    ("Coins.Get(bitcoin)", async () =>
    {
        var r = await gecko.Coins.GetAsync("bitcoin");
        Console.WriteLine($"    → {r.Name} ({r.Symbol?.ToUpperInvariant()})  rank {r.MarketCapRank}  genesis {r.GenesisDate}");
    }),

    ("Coins.GetMarketChart(BTC,7d)", async () =>
    {
        var r = await gecko.Coins.GetMarketChartAsync("bitcoin", "usd", MarketChartRange.SevenDays);
        Console.WriteLine($"    → {r.Prices.Count} price points, first {r.Prices[0].Timestamp:yyyy-MM-dd HH:mm} → ${r.Prices[0].Value:N2}");
    }),

    ("Coins.GetOhlc(BTC,1d)", async () =>
    {
        var r = await gecko.Coins.GetOhlcAsync("bitcoin", "usd", MarketChartRange.OneDay);
        Console.WriteLine($"    → {r.Count} candles, last close ${r[^1].Close:N2}");
    }),

    ("Search(ethereum)", async () =>
    {
        var r = await gecko.Search.SearchAsync("ethereum");
        Console.WriteLine($"    → {r.Coins.Count} coins, {r.Exchanges.Count} exchanges, {r.Nfts.Count} NFTs, {r.Categories.Count} categories");
    }),

    ("Trending.Get", async () =>
    {
        var r = await gecko.Trending.GetAsync();
        Console.WriteLine($"    → {r.Coins.Count} trending coins (top: {r.Coins[0].Item?.Name}), {r.Nfts.Count} NFTs, {r.Categories.Count} categories");
    }),

    ("Categories.GetList", async () =>
    {
        var r = await gecko.Categories.GetListAsync();
        Console.WriteLine($"    → {r.Count} categories, first: {r[0].Name}");
    }),

    ("Categories.Get", async () =>
    {
        var r = await gecko.Categories.GetAsync();
        Console.WriteLine($"    → {r.Count} with market data, top: {r[0].Name} (${r[0].MarketCap:N0})");
    }),

    ("AssetPlatforms.GetList", async () =>
    {
        var r = await gecko.AssetPlatforms.GetListAsync();
        Console.WriteLine($"    → {r.Count} asset platforms, first: {r[0].Name}");
    }),

    ("Exchanges.Get(top 5)", async () =>
    {
        var r = await gecko.Exchanges.GetAsync(new ExchangesOptions { PerPage = 5 });
        Console.WriteLine($"    → {r.Count} exchanges, top: {r[0].Name} (trust {r[0].TrustScore})");
    }),

    ("Exchanges.GetList", async () =>
    {
        var r = await gecko.Exchanges.GetListAsync();
        Console.WriteLine($"    → {r.Count} exchange ids");
    }),

    ("Derivatives.GetTickers", async () =>
    {
        var r = await gecko.Derivatives.GetTickersAsync();
        Console.WriteLine($"    → {r.Count} derivative tickers, first: {r[0].Market} {r[0].Symbol} @ {r[0].Price}");
    }),

    ("Global.Get", async () =>
    {
        var r = await gecko.Global.GetAsync();
        var mcap = r.TotalMarketCap?.TryGetValue("usd", out var v) == true ? v : 0m;
        Console.WriteLine($"    → {r.ActiveCryptocurrencies} coins, {r.Markets} markets, total mcap (USD): ${mcap:N0}");
    }),

    ("Global.GetDefi", async () =>
    {
        var r = await gecko.Global.GetDefiAsync();
        Console.WriteLine($"    → DeFi mcap ${r.DefiMarketCap}, top coin: {r.TopCoinName}");
    }),

    ("Nfts.GetList(10)", async () =>
    {
        var r = await gecko.Nfts.GetListAsync(10, 1);
        Console.WriteLine($"    → {r.Count} NFTs, first: {r[0].Name}");
    }),

    ("Onchain.GetNetworks", async () =>
    {
        var r = await gecko.Onchain.GetNetworksAsync();
        Console.WriteLine($"    → {r.Length} networks, first: {r[0].Attributes?.Name}");
    }),

    ("Onchain.GetTrendingPools", async () =>
    {
        var r = await gecko.Onchain.GetTrendingPoolsAsync(new()
        {
            Include = ["base_token", "quote_token"],
        });
        Console.WriteLine($"    → {r.Length} trending pools, top: {r[0].Attributes?.Name}");
    }),

    ("Onchain.SearchPools(doge)", async () =>
    {
        var r = await gecko.Onchain.SearchPoolsAsync("doge");
        Console.WriteLine($"    → {r.Length} pool hits, top: {r[0].Attributes?.Name}");
    }),
};

Console.WriteLine($"Running {checks.Length} smoke-test calls against Demo plan...\n");

var passed = 0;
var failed = 0;
var sw = System.Diagnostics.Stopwatch.StartNew();

foreach (var (label, run) in checks)
{
    var start = sw.Elapsed;
    Console.Write($"• {label,-35} ");
    try
    {
        await run();
        Console.WriteLine($"  ({(sw.Elapsed - start).TotalMilliseconds:N0} ms)");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  FAILED: {ex.GetType().Name}: {ex.Message}");
        failed++;
    }
}

Console.WriteLine();
Console.WriteLine($"=== {passed}/{checks.Length} passed, {failed} failed — {sw.Elapsed.TotalSeconds:N1}s total ===");
return failed == 0 ? 0 : 1;
