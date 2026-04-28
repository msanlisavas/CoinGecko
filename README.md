# CoinGecko

A production-grade, strongly typed, AOT-safe **.NET client suite** for the [CoinGecko](https://www.coingecko.com/) API.

Four packages — install only what you need:

| Package | NuGet | Downloads | Purpose |
|---|---|---|---|
| **CoinGecko.Api** | [![NuGet](https://img.shields.io/nuget/v/CoinGecko.Api.svg?label=v&color=blue)](https://www.nuget.org/packages/CoinGecko.Api) | [![Downloads](https://img.shields.io/nuget/dt/CoinGecko.Api.svg?color=blue)](https://www.nuget.org/packages/CoinGecko.Api) | REST core — 15 sub-clients, 100+ endpoints |
| **CoinGecko.Api.WebSockets** | [![NuGet](https://img.shields.io/nuget/vpre/CoinGecko.Api.WebSockets.svg?label=v&color=orange)](https://www.nuget.org/packages/CoinGecko.Api.WebSockets) | [![Downloads](https://img.shields.io/nuget/dt/CoinGecko.Api.WebSockets.svg?color=orange)](https://www.nuget.org/packages/CoinGecko.Api.WebSockets) | Streaming beta (WSS, 4 channels) |
| **CoinGecko.Api.AiAgentHub** | [![NuGet](https://img.shields.io/nuget/vpre/CoinGecko.Api.AiAgentHub.svg?label=v&color=orange)](https://www.nuget.org/packages/CoinGecko.Api.AiAgentHub) | [![Downloads](https://img.shields.io/nuget/dt/CoinGecko.Api.AiAgentHub.svg?color=orange)](https://www.nuget.org/packages/CoinGecko.Api.AiAgentHub) | `Microsoft.Extensions.AI` function tools |
| **CoinGecko.Api.AiAgentHub.Mcp** | [![NuGet](https://img.shields.io/nuget/vpre/CoinGecko.Api.AiAgentHub.Mcp.svg?label=v&color=orange)](https://www.nuget.org/packages/CoinGecko.Api.AiAgentHub.Mcp) | [![Downloads](https://img.shields.io/nuget/dt/CoinGecko.Api.AiAgentHub.Mcp.svg?color=orange)](https://www.nuget.org/packages/CoinGecko.Api.AiAgentHub.Mcp) | MCP client → hosted CoinGecko MCP |

[![CI](https://github.com/msanlisavas/CoinGecko/actions/workflows/ci.yml/badge.svg)](https://github.com/msanlisavas/CoinGecko/actions/workflows/ci.yml)
[![CodeQL](https://github.com/msanlisavas/CoinGecko/actions/workflows/codeql.yml/badge.svg)](https://github.com/msanlisavas/CoinGecko/actions/workflows/codeql.yml)
[![.NET 9 + .NET 8 (LTS)](https://img.shields.io/badge/.NET-9.0%20%7C%208.0%20LTS-512BD4)](#compatibility-matrix)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Why this library

- **Complete API coverage** — every documented CoinGecko REST endpoint on core + onchain (GeckoTerminal) surfaces, plus the WebSocket beta stream and the hosted MCP server.
- **Strong typing end-to-end** — typed options records per endpoint, `[Flags]` + `[EnumMember]` enums for closed-set params, discriminated plan tiers, and a 7-exception hierarchy with correlation IDs.
- **Resilient by default** — built-in 429 `Retry-After` handler + exponential-backoff retry for transient 5xx / socket errors. No Polly dependency; consumers opt in by chaining `AddStandardResilienceHandler()` if they want more.
- **Plan-aware** — `[RequiresPlan(...)]` attribute gates higher-tier endpoints *before* the HTTP call, throwing `CoinGeckoPlanException` with the required tier. No wasted credits on unauthorized calls.
- **AOT + trim-safe** — source-generated `System.Text.Json`, zero reflection on the hot path. Validated via a Blazor WebAssembly sample with `TrimMode=full`.
- **DI-first** — `AddCoinGeckoApi(...)` returns `IHttpClientBuilder` so consumers can layer their own handlers (custom resilience, logging, distributed tracing).
- **Observability included** — `ActivitySource("CoinGecko.Api")` + `LoggerMessage` source-gen events; zero forced OpenTelemetry dependency.
- **Two ways to plug into agents** — `CoinGecko.Api.AiAgentHub` wraps the REST client as `AIFunction` tools; `CoinGecko.Api.AiAgentHub.Mcp` fetches CoinGecko's hosted MCP tools. Same `IReadOnlyList<AIFunction>` return type — swap in one line.

---

## Install

Only install what you use. They compose:

```powershell
# REST core (most common case)
dotnet add package CoinGecko.Api

# Opt-in extensions (preview):
dotnet add package CoinGecko.Api.WebSockets          # streaming
dotnet add package CoinGecko.Api.AiAgentHub          # MEAI tools backed by REST
dotnet add package CoinGecko.Api.AiAgentHub.Mcp      # MCP client to CoinGecko's hosted MCP
```

---

## Quickstart — REST

### ASP.NET Core / Generic Host

```csharp
using CoinGecko.Api;

builder.Services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = builder.Configuration["CoinGecko:ApiKey"];
    opts.Plan   = CoinGeckoPlan.Demo; // Demo / Basic / Analyst / Lite / Pro / ProPlus / Enterprise
});

// Inject ICoinGeckoClient anywhere:
public sealed class PriceService(ICoinGeckoClient gecko)
{
    public Task<Coin> GetBtcAsync(CancellationToken ct)
        => gecko.Coins.GetAsync("bitcoin", ct: ct);
}
```

### Console / scripts

```csharp
using CoinGecko.Api;

using var scope = CoinGeckoClientFactory.Create("my-api-key", CoinGeckoPlan.Pro);
var btc = await scope.Client.Coins.GetAsync("bitcoin");
Console.WriteLine($"BTC: ${btc.MarketData!.CurrentPrice!["usd"]:N2}");
```

### Auto-paginated enumeration

List endpoints expose an `IAsyncEnumerable<T>` sibling that walks pages until exhaustion:

```csharp
await foreach (var m in gecko.Coins.EnumerateMarketsAsync("usd"))
{
    Console.WriteLine($"{m.MarketCapRank,4}  {m.Symbol,-6}  ${m.CurrentPrice,10:N2}");
}
```

### Error handling

```csharp
try
{
    var data = await gecko.Coins.GetOhlcRangeAsync(/*...*/); // Analyst+ endpoint
}
catch (CoinGeckoPlanException ex)       { /* current plan too low; ex.RequiredPlan / ex.ActualPlan */ }
catch (CoinGeckoRateLimitException ex)  { /* 429; ex.RetryAfter available */ }
catch (CoinGeckoAuthException ex)       { /* 401 / 403 */ }
catch (CoinGeckoNotFoundException)      { /* 404 — unknown coin id etc. */ }
catch (CoinGeckoValidationException ex) { /* 400 */ }
catch (CoinGeckoServerException ex)     { /* 5xx after retries */ }
catch (CoinGeckoException ex)           { /* base type */ }
```

Every exception exposes `StatusCode`, `RawBody`, and `RequestId` (correlates with `ActivitySource` / `ILogger` events).

---

## Sub-clients

Accessed from `ICoinGeckoClient`:

| Sub-client | Endpoints covered |
|---|---|
| `Coins` | list · markets · detail · tickers · history · market_chart · market_chart/range · OHLC · OHLC/range (A+) · supply charts (A+) · contract lookups · top gainers/losers (A+) · new listings (P+) |
| `Nfts` | list · detail · contract lookup · markets (B+) · market_chart (B+) · tickers (B+) |
| `Exchanges` | list · detail · tickers · volume_chart · volume_chart/range (B+) |
| `Derivatives` | tickers · exchanges list · exchanges detail · exchanges id-map |
| `Categories` | list · with market data |
| `AssetPlatforms` | list · token lists (B+) |
| `Companies` | public treasury by coin (B+) |
| `Simple` | price · token_price by contract · supported vs_currencies |
| `Global` | global market · DeFi · market_cap_chart (B+) |
| `Search` | coin/NFT/exchange/category search |
| `Trending` | trending coins + NFTs + categories |
| `Onchain` | 29 GeckoTerminal endpoints — networks, DEXes, pools, tokens, OHLCV, trades, simple token price, search, categories (B+/A+); top holders with optional PnL details (B+) |
| `Key` | plan + rate limit + credits remaining (B+) |
| `News` | crypto news + guides aggregated from 100+ publishers (A+) |
| `Ping` | health check |

_A+ = Analyst or higher, B+ = Basic or higher (any paid plan), P+ = Pro or higher._

Onchain token / pool models surface the new **OTV** fields (`outstanding_supply`, `outstanding_token_value_usd`) and the **`gt_verified`** flag on token + pool attributes.

Every tier gate is enforced client-side by `[RequiresPlan]` — attempts to call a Pro-only method with a Demo key throw `CoinGeckoPlanException` before issuing the HTTP request.

---

## Streaming (`CoinGecko.Api.WebSockets`)

Real-time feeds via CoinGecko's WebSocket beta. Analyst+ plan required. Four typed channels:

| Channel | Tick type | Purpose |
|---|---|---|
| C1 `CGSimplePrice` | `CoinPriceTick` | CoinGecko aggregated coin prices |
| G1 `OnchainSimpleTokenPrice` | `OnchainTokenPriceTick` | GeckoTerminal token prices |
| G2 `OnchainTrade` | `DexTrade` | DEX pool swaps |
| G3 `OnchainOHLCV` | `OnchainOhlcvCandle` | DEX pool OHLCV candles |

```csharp
using CoinGecko.Api.WebSockets;
using Microsoft.Extensions.DependencyInjection;

services.AddCoinGeckoStream(opts =>
{
    opts.ApiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY");
    opts.AutoReconnect = true;                              // default
    opts.MaxReconnectAttempts = 10;                         // default
    opts.HeartbeatTimeout = TimeSpan.FromSeconds(25);       // default
    opts.MaxSubscriptionsPerChannel = 100;                  // default (mirrors upstream cap)
});

var stream = sp.GetRequiredService<ICoinGeckoStream>();
stream.StateChanged += (_, e) => Console.WriteLine($"{e.Previous} → {e.Current}");

await stream.ConnectAsync();

var sub = await stream.SubscribeCoinPricesAsync(
    coinIds: ["bitcoin", "ethereum"],
    vsCurrencies: ["usd"],
    onTick: tick => Console.WriteLine(
        $"{tick.CoinId}: ${tick.Price:N2} ({tick.PricePercentChange24h:+0.00;-0.00}%)"));

// Later:
await sub.DisposeAsync();               // unsubscribe this subscription
await stream.DisconnectAsync();         // close the socket
```

Features: auto-reconnect with decorrelated-jitter exponential backoff, heartbeat watchdog, automatic resubscription on reconnect, subscription-cap enforcement.

Runnable example: [`samples/CoinGecko.Api.Samples.StreamConsole`](samples/CoinGecko.Api.Samples.StreamConsole).

---

## AI agent tools via REST (`CoinGecko.Api.AiAgentHub`)

Wrap the REST client as `Microsoft.Extensions.AI` function tools, consumable by any `IChatClient` (OpenAI, Anthropic, Azure OpenAI, Ollama, Gemini, Bedrock — anything MEAI-compatible).

```csharp
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using Microsoft.Extensions.AI;

var tools = CoinGeckoAiTools.Create(gecko, new CoinGeckoAiToolsOptions
{
    Tools = CoinGeckoToolSet.CoinPrices | CoinGeckoToolSet.Trending,
    MaxResults = 25,
    ToolFilter = name => !name.Contains("onchain"),         // optional custom filter
});

IChatClient chat = /* any IChatClient */;
var response = await chat.GetResponseAsync(
    "What's BTC at right now and what's trending?",
    new ChatOptions { Tools = tools.Cast<AITool>().ToArray() });
```

Nine built-in tools across eight categories (`CoinPrices`, `CoinSearch`, `MarketData`, `Trending`, `Categories`, `Nfts`, `Derivatives`, `Onchain`). Each tool emits a **compact projection** (e.g. `CoinPriceQuote`, `MarketSnapshot`) rather than the full DTO, to conserve LLM context.

Runnable example: [`samples/CoinGecko.Api.Samples.AgentDemo`](samples/CoinGecko.Api.Samples.AgentDemo).

---

## AI agent tools via MCP (`CoinGecko.Api.AiAgentHub.Mcp`)

Fetch tool definitions directly from CoinGecko's hosted [MCP](https://modelcontextprotocol.io/) server — no REST client involved. Same `IReadOnlyList<AIFunction>` return type, so swapping between REST-backed tools and MCP tools is a one-line change.

```csharp
using CoinGecko.Api.AiAgentHub.Mcp;
using Microsoft.Extensions.AI;

var tools = await CoinGeckoMcp.ConnectAsync(
    apiKey: Environment.GetEnvironmentVariable("COINGECKO_API_KEY")!,
    plan:   CoinGeckoPlan.Pro);

IChatClient chat = /* any IChatClient */;
var response = await chat.GetResponseAsync(
    "What's BTC at right now?",
    new ChatOptions { Tools = tools.Cast<AITool>().ToArray() });
```

Need finer control? `CoinGeckoMcp.CreateClientAsync(...)` returns the underlying MCP client directly. Configurable transport (`StreamableHttp` default, `Sse` fallback), per-call timeout, and optional `BaseAddress` override for proxied / enterprise setups.

Runnable example: [`samples/CoinGecko.Api.Samples.McpAgent`](samples/CoinGecko.Api.Samples.McpAgent).

---

## Plan compatibility

| Plan | Base URL | Endpoints available |
|---|---|---|
| **Demo** (free) | `api.coingecko.com/api/v3/` | Public + Demo-tier endpoints (~70% of the surface) |
| **Basic / Analyst / Lite / Pro / Pro+ / Enterprise** | `pro-api.coingecko.com/api/v3/` | All documented endpoints; higher-tier methods gated by `[RequiresPlan]` |

Configure via `CoinGeckoOptions.Plan`; the library picks the correct base URL automatically. Override with `CoinGeckoOptions.BaseAddress` if you proxy through your own infrastructure.

---

## Compatibility matrix

|  | `CoinGecko.Api` | `.WebSockets` | `.AiAgentHub` | `.AiAgentHub.Mcp` |
|---|:---:|:---:|:---:|:---:|
| .NET 8.0 (LTS) | ✅ | ✅ | ✅ | ✅ |
| .NET 9.0 | ✅ | ✅ | ✅ | ✅ |
| NativeAOT | ✅ | ✅ | ⚠️ | ⚠️ |
| Trimming | ✅ | ✅ | ⚠️ | ⚠️ |
| .NET Framework | ❌ | ❌ | ❌ | ❌ |

⚠️ = works but emits IL2026 / IL3050 warnings because the underlying `AIFunctionFactory` / MCP SDK uses reflection. A source-generated AOT-safe alternative is planned for the v0.2 line of the agent packages.

---

## Samples

Five runnable examples under [`samples/`](samples):

- **`CoinGecko.Api.Samples.Console`** — minimal REST quickstart: ping, `GetMarketsAsync`, `EnumerateMarketsAsync`.
- **`CoinGecko.Api.Samples.Blazor`** — Blazor WebAssembly with `PublishTrimmed=true`, `TrimMode=full` — the AOT / trim-safety proof.
- **`CoinGecko.Api.Samples.StreamConsole`** — WebSocket streaming printing live BTC/ETH prices.
- **`CoinGecko.Api.Samples.AgentDemo`** — MEAI tool discovery + one canned invocation (no LLM key required).
- **`CoinGecko.Api.Samples.McpAgent`** — Connects to CoinGecko's hosted MCP, prints the fetched tool catalog.

---

## Development

```bash
# Restore + build + test everything
dotnet restore
dotnet build -c Release
dotnet test  -c Release

# Pack a single package locally
dotnet pack src/CoinGecko.Api/CoinGecko.Api.csproj -c Release -o artifacts

# Run any sample (env var provides the key)
COINGECKO_API_KEY=your-demo-key dotnet run --project samples/CoinGecko.Api.Samples.Console
```

### Repo layout

```
src/                    Four NuGet-packable projects (one per published package)
tests/                  xUnit v3 unit + WireMock mock + Kestrel-hosted WS integration tests
samples/                Five runnable samples
docs/                   API research + design spec + per-package implementation plans
.github/workflows/      CI + release + CodeQL + Dependabot
eng/                    Shared build assets (icon)
```

### CI / release

- **`ci.yml`** — matrix build + test on ubuntu / windows / macos × (net8.0, net9.0) on every push and PR. Format check (`dotnet format --verify-no-changes`) on Ubuntu.
- **`codeql.yml`** — weekly + PR-gated C# security scan.
- **`dependabot.yml`** — weekly NuGet + GitHub Actions dep updates.
- **`release.yml`** — tag-triggered NuGet publish, one tag per package:

| Tag pattern | Publishes |
|---|---|
| `v{semver}-api` | `CoinGecko.Api` |
| `v{semver}-preview-websockets` | `CoinGecko.Api.WebSockets` |
| `v{semver}-preview-aiagenthub` | `CoinGecko.Api.AiAgentHub` |
| `v{semver}-preview-mcp` | `CoinGecko.Api.AiAgentHub.Mcp` |

Each release runs under the GitHub **`Production`** environment and requires the `NUGET_API_KEY` secret configured there. Stable `-api` releases are published as regular releases; any `-preview-*` tag is automatically marked as a GitHub prerelease.

Per-package release notes and breaking-change details live in [CHANGELOG.md](CHANGELOG.md).

---

## Contributing

Bugs, missing endpoints, or protocol drift from upstream CoinGecko — please open an issue or PR. For new endpoints, follow the canonical sub-client pattern established in `src/CoinGecko.Api/Resources/PingClient.cs`.

Pre-PR checklist:

- `dotnet format --verify-no-changes`
- `dotnet build -c Release` — 0 warnings, 0 errors
- `dotnet test -c Release` — all green
- New public API surface added to `src/CoinGecko.Api/PublicAPI.Unshipped.txt`

---

## License

[MIT](LICENSE) © msanlisavas

---

## Buy me a coffee?

If this library saves you time, a tip in USDT (TRC20, on Tron) is appreciated:

```
TGgenJpoPvTFsd61hCUquyM4yQ9mAmD9hc
```
