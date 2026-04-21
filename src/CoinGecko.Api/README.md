# CoinGecko.Api

Strongly typed, AOT-safe .NET client for the [CoinGecko](https://www.coingecko.com/) REST API.

## Install

```powershell
dotnet add package CoinGecko.Api
```

## Quickstart (ASP.NET Core / Minimal Hosting)

```csharp
builder.Services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = builder.Configuration["CoinGecko:ApiKey"];
    opts.Plan   = CoinGeckoPlan.Demo; // or Analyst / Lite / Pro / ProPlus / Enterprise
});

// Inject ICoinGeckoClient anywhere:
public sealed class PriceService(ICoinGeckoClient gecko)
{
    public Task<Coin> GetBtcAsync(CancellationToken ct)
        => gecko.Coins.GetAsync("bitcoin", ct: ct);
}
```

## Quickstart (Console / scripts)

```csharp
using var gecko = CoinGeckoClientFactory.Create("my-api-key", CoinGeckoPlan.Pro);
var btc = await gecko.Coins.GetAsync("bitcoin");
Console.WriteLine($"BTC: ${btc.MarketData.CurrentPrice["usd"]}");
```

See the [full documentation](https://github.com/msanlisavas/CoinGecko) for all 14 sub-clients, streaming (CoinGecko.Api.WebSockets), and AI Agent Hub integrations (CoinGecko.Api.AiAgentHub, CoinGecko.Api.AiAgentHub.Mcp).

## License

MIT © msanlisavas
