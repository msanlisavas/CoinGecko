using CoinGecko.Api;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY");
    opts.Plan = CoinGeckoPlan.Demo;
});

using var sp = services.BuildServiceProvider();
var gecko = sp.GetRequiredService<ICoinGeckoClient>();

try
{
    var ping = await gecko.Ping.PingAsync();
    Console.WriteLine($"Ping: {ping.GeckoSays}");
    Console.WriteLine();

    Console.WriteLine("Top 10 coins by market cap (USD):");
    Console.WriteLine("Rank  Symbol  Price        Name");
    Console.WriteLine("----  ------  -----------  ---------------");

    var markets = await gecko.Coins.GetMarketsAsync("usd",
        new() { PerPage = 10, Page = 1 });

    foreach (var m in markets)
    {
        Console.WriteLine($"{m.MarketCapRank,4}  {m.Symbol?.ToUpperInvariant(),-6}  ${m.CurrentPrice,10:N2}  {m.Name}");
    }

    Console.WriteLine();
    Console.WriteLine("Streaming top 25 via EnumerateMarketsAsync:");
    var count = 0;
    await foreach (var m in gecko.Coins.EnumerateMarketsAsync("usd"))
    {
        Console.WriteLine($"  {m.MarketCapRank,4}  {m.Symbol?.ToUpperInvariant(),-6}  ${m.CurrentPrice,10:N2}");
        if (++count >= 25) { break; }
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

return 0;
