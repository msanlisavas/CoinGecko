# CoinGecko.Api.WebSockets

Streaming client for [CoinGecko](https://www.coingecko.com/)'s beta WebSocket API. Analyst+ plan required.

## Install

```powershell
dotnet add package CoinGecko.Api.WebSockets
```

## Quickstart

```csharp
builder.Services.AddCoinGeckoStream(opts =>
{
    opts.ApiKey = builder.Configuration["CoinGecko:ApiKey"];
    opts.AutoReconnect = true;
});

// Later, in a hosted service:
public sealed class PriceWatcher(ICoinGeckoStream stream)
{
    public async Task StartAsync(CancellationToken ct)
    {
        await stream.ConnectAsync(ct);

        var sub = await stream.SubscribeCoinPricesAsync(
            coinIds: ["bitcoin", "ethereum"],
            onTick: tick => Console.WriteLine($"{tick.CoinId}: ${tick.Price:N2}"),
            ct);
    }
}
```

## Preview status

CoinGecko's WebSocket API is itself in beta. This package ships in `0.x` until the upstream protocol stabilizes. Expect breaking changes across minor versions.

## License

MIT © msanlisavas
