# CoinGecko

[![CI](https://github.com/msanlisavas/CoinGecko/actions/workflows/ci.yml/badge.svg)](https://github.com/msanlisavas/CoinGecko/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/CoinGecko.Api.svg)](https://www.nuget.org/packages/CoinGecko.Api)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Strongly typed, AOT-safe .NET client for the [CoinGecko](https://www.coingecko.com/) API. Covers every documented REST endpoint on core and onchain (GeckoTerminal) surfaces, with async pagination, plan-aware gating, resilient rate-limit handling, and first-class DI integration.

## Packages

| Package | Purpose | Status |
|---|---|---|
| `CoinGecko.Api` | REST core (14 sub-clients, 100+ endpoints) | **v0.1.0** |
| `CoinGecko.Api.WebSockets` | Streaming beta client (ActionCable over WSS) | Planned |
| `CoinGecko.Api.AiAgentHub` | `Microsoft.Extensions.AI` function tools | Planned |
| `CoinGecko.Api.AiAgentHub.Mcp` | MCP client for CoinGecko's hosted MCP server | Planned |

## Install

```powershell
dotnet add package CoinGecko.Api
```

## Quickstart — ASP.NET Core / Minimal Hosting

```csharp
builder.Services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = builder.Configuration["CoinGecko:ApiKey"];
    opts.Plan   = CoinGeckoPlan.Demo; // Demo / Basic / Analyst / Lite / Pro / ProPlus / Enterprise
});

public sealed class PriceService(ICoinGeckoClient gecko)
{
    public Task<Coin> GetBtcAsync(CancellationToken ct)
        => gecko.Coins.GetAsync("bitcoin", ct: ct);
}
```

## Quickstart — Console / scripts

```csharp
using var scope = CoinGeckoClientFactory.Create("my-api-key", CoinGeckoPlan.Pro);
var btc = await scope.Client.Coins.GetAsync("bitcoin");
Console.WriteLine($"BTC: ${btc.MarketData!.CurrentPrice!["usd"]}");
```

## Sub-clients

| Sub-client | Purpose |
|---|---|
| `Ping` | API reachability + credentials probe |
| `Coins` | Coin list, markets, detail, history, market_chart, OHLC, supply charts, contract lookups, top gainers/losers |
| `Nfts` | NFT collections, markets, market chart, tickers, contract lookups |
| `Exchanges` | Exchange list, detail, tickers, volume chart |
| `Derivatives` | Derivative tickers + exchanges |
| `Categories` | Category list + categories with market data |
| `AssetPlatforms` | Supported chains + token lists |
| `Companies` | Public-treasury holdings by coin |
| `Simple` | `/simple/price`, `/simple/token_price/{id}`, supported vs_currencies |
| `Global` | Global market + DeFi snapshots + market-cap chart |
| `Search` | Coin/category/exchange/NFT search |
| `Trending` | Trending coins, NFTs, categories |
| `Onchain` | GeckoTerminal: networks, DEXes, pools, tokens, OHLCV, trades, categories |
| `Key` | API key usage + remaining credits |

## Plan compatibility

| Plan | Base URL | Endpoints available |
|---|---|---|
| Demo (free) | `api.coingecko.com/api/v3/` | ~70% of documented endpoints |
| Basic / Analyst / Lite / Pro / Pro+ / Enterprise | `pro-api.coingecko.com/api/v3/` | All documented endpoints; higher-tier features gated at the method level |

Plan gating is enforced client-side via `[RequiresPlan]` on sub-client methods — calls to a Pro-tier endpoint with a Demo key throw `CoinGeckoPlanException` before the HTTP request is issued.

## Compatibility matrix

| | net8.0 (LTS) | net9.0 | NativeAOT | Trimming |
|---|---|---|---|---|
| `CoinGecko.Api` | ✅ | ✅ | ✅ | ✅ |

## Samples

- [`samples/CoinGecko.Api.Samples.Console`](samples/CoinGecko.Api.Samples.Console) — minimal Console quickstart.
- [`samples/CoinGecko.Api.Samples.Blazor`](samples/CoinGecko.Api.Samples.Blazor) — Blazor WASM with trimming enabled, proving trim-safety.

## Development

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet pack src/CoinGecko.Api/CoinGecko.Api.csproj -c Release -o artifacts
```

## License

MIT © msanlisavas
