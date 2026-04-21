using CoinGecko.Api.WebSockets;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoinGeckoStream(opts =>
{
    opts.ApiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY");
});
using var sp = services.BuildServiceProvider();

var stream = sp.GetRequiredService<ICoinGeckoStream>();
stream.StateChanged += (_, e) => Console.WriteLine($"[state] {e.Previous} -> {e.Current}");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { cts.Cancel(); e.Cancel = true; };

await stream.ConnectAsync(cts.Token);

var sub = await stream.SubscribeCoinPricesAsync(
    ["bitcoin", "ethereum"], ["usd"],
    tick => Console.WriteLine($"{DateTimeOffset.Now:HH:mm:ss}  {tick.CoinId,-10}  ${tick.Price,12:N2}  ({tick.PricePercentChange24h,+6:N2}%)"),
    cts.Token);

try { await Task.Delay(Timeout.Infinite, cts.Token); } catch { }
await sub.DisposeAsync();
await stream.DisconnectAsync(CancellationToken.None);
