# CoinGecko .NET Wrapper — Design Spec

_Date: 2026-04-21 · Owner: @msanlisavas · Status: approved, pending implementation plan_

Production-grade C# client library for the CoinGecko API, intended for publication on NuGet. Covers the full REST surface (core + onchain), the WebSocket beta stream, and the AI Agent Hub (Microsoft.Extensions.AI tool bindings + hosted-MCP client).

This spec references the API catalog produced from upstream docs on the same date: [`docs/coingecko-api-research.md`](../../coingecko-api-research.md). That document is the source of truth for endpoint inventories, rate-limit tables, and protocol quirks; this document is the source of truth for library design decisions.

---

## 1. Goals & non-goals

### Goals

1. Idiomatic, strongly-typed .NET client that looks and behaves like a first-party SDK (comparable in polish to `Octokit`, `Stripe.net`, the Azure SDK, or the official OpenAI .NET SDK).
2. Full coverage of CoinGecko's published REST surface: core (coins / nfts / exchanges / derivatives / simple / categories / asset-platforms / companies / search / trending / global / ping / key) and onchain / GeckoTerminal (networks / pools / tokens / dexes / OHLCV / trades / simple token price).
3. Full coverage of the WebSocket Beta stream (channels C1 / G1 / G2 / G3).
4. Two opt-in integration paths for agents:
   - Microsoft.Extensions.AI `AIFunction` tools backed by the REST client.
   - `ModelContextProtocol` client wrapping CoinGecko's hosted MCP server.
5. Fully AOT- and trim-safe across all shipped packages (no reflection-based JSON, no dynamic tool generation on the hot path).
6. DI-first public API, with a static factory as an affordance for scripts and console apps.
7. Clean separation of concerns: stable REST client can ship 1.0 while the beta WebSocket and evolving MCP bridge stay in 0.x without blocking it.
8. "Batteries included" default resilience for CoinGecko's specific behaviors (429 + `Retry-After`, plan-gated endpoints) with zero Polly transitive dependency.
9. Ergonomics that exceed a hand-rolled wrapper: `IAsyncEnumerable` auto-pagination, typed option records per endpoint, observability via `ActivitySource` + `LoggerMessage` source-gen, clean exception hierarchy, cancellation everywhere.

### Non-goals

- Data caching, portfolio modeling, indicator calculation, or anything above the HTTP/WS transport layer.
- Exchange trading, wallet signing, on-chain RPC calls (handled by other libraries).
- Non-CoinGecko price sources, aggregation across providers, fallback to free tiers on Pro-key failures.
- Shipping a hosted / self-host MCP *server* (CoinGecko already hosts one; our MCP subpackage is a client).
- .NET Framework 4.x support. netstandard2.0 is explicitly not a target (see §2).
- Newtonsoft.Json, RestSharp, Refit, or any third-party HTTP / serialization layer as a dependency of the core package.

---

## 2. Packages & target frameworks

### Package topology

| Package ID | Purpose | Depends on | Initial version |
|---|---|---|---|
| `CoinGecko.Api` | REST core (all core + onchain endpoints, DI, handler pipeline) | `Microsoft.Extensions.Http`, `Microsoft.Extensions.Options` | `0.1.0` → `1.0.0` when API surface stabilizes |
| `CoinGecko.Api.WebSockets` | Streaming beta client (ActionCable envelope, four channels) | `CoinGecko.Api` | `0.1.0`, stays in 0.x while upstream is beta |
| `CoinGecko.Api.AiAgentHub` | Microsoft.Extensions.AI `AIFunction` tools backed by `ICoinGeckoClient` | `CoinGecko.Api`, `Microsoft.Extensions.AI.Abstractions` | Follows core |
| `CoinGecko.Api.AiAgentHub.Mcp` | Client for CoinGecko's hosted MCP server → `IReadOnlyList<AIFunction>` | `ModelContextProtocol`, `Microsoft.Extensions.AI.Abstractions` | `0.1.0`, stays in 0.x until MCP spec + MEAI MCP bridge stabilize |

Dependency graph (no cycles):

```
CoinGecko.Api
   ↑
   ├── CoinGecko.Api.WebSockets
   └── CoinGecko.Api.AiAgentHub
        (sibling, not parent)
CoinGecko.Api.AiAgentHub.Mcp     (no dep on .Api; depends on MCP SDK + MEAI abstractions)
```

Each package versions independently. Breaking changes in `.WebSockets` or `.AiAgentHub.Mcp` must not force a major bump of `.Api`.

### Target frameworks

All four projects: `<TargetFrameworks>net9.0;net8.0</TargetFrameworks>`.

- `net9.0` is the current release; `net8.0` is LTS through Nov 2026.
- `netstandard2.0` is explicitly rejected. The cost (polyfills for `IAsyncEnumerable`, `ValueTask`, nullable attribute shims, no source-gen STJ) is not justified by the tiny remaining audience (Unity, .NET Framework, old Xamarin).
- `<IsAotCompatible>true</IsAotCompatible>` in every project. CI builds run `dotnet publish -r linux-x64 /p:PublishAot=true` on a smoke sample to catch regressions.

---

## 3. Repo layout

```
CoinGecko/                                      (repo root; remote: github.com/msanlisavas/CoinGecko)
├─ .editorconfig, .gitignore, .gitattributes
├─ Directory.Build.props                        lang=latest, nullable, treat-warnings-as-errors, deterministic, SourceLink, embed sources
├─ Directory.Packages.props                     Central Package Management
├─ global.json                                  .NET 9 SDK, rollForward: latestFeature
├─ README.md                                    user-facing quickstart
├─ LICENSE                                      MIT
├─ CoinGecko.sln
├─ .github/
│  ├─ workflows/
│  │  ├─ ci.yml                                 build + test, ubuntu/windows/macos × (net8, net9), all test projects
│  │  ├─ release.yml                            tag-triggered pack + push to nuget.org, symbols to nuget symbol server
│  │  └─ codeql.yml                             security scan on PRs
│  ├─ dependabot.yml                            weekly, NuGet + actions
│  └─ ISSUE_TEMPLATE/                           bug / feature / docs
├─ docs/
│  ├─ coingecko-api-research.md                 API catalog (source of truth for endpoints)
│  ├─ superpowers/specs/                        design docs (this file)
│  └─ api/                                      DocFX output (gitignored; rebuilt on each release, published to GH Pages)
├─ eng/
│  ├─ icon.png                                  NuGet package icon
│  └─ (no .snk — packages are not strong-named; matches modern convention)
├─ src/
│  ├─ CoinGecko.Api/
│  ├─ CoinGecko.Api.WebSockets/
│  ├─ CoinGecko.Api.AiAgentHub/
│  └─ CoinGecko.Api.AiAgentHub.Mcp/
├─ tests/
│  ├─ CoinGecko.Api.Tests/                      unit (xUnit v3 + Shouldly + NSubstitute)
│  ├─ CoinGecko.Api.ContractTests/              snapshot serialization (Verify)
│  ├─ CoinGecko.Api.MockTests/                  WireMock E2E
│  ├─ CoinGecko.Api.WebSockets.Tests/           fake WS server via Kestrel
│  ├─ CoinGecko.Api.AiAgentHub.Tests/           tool registration + MCP client against MCP SDK fake server
│  └─ CoinGecko.Api.SmokeTests/                 opt-in, hits real CoinGecko, excluded from CI
└─ samples/
   ├─ CoinGecko.Api.Samples.Console/            REST quickstart
   ├─ CoinGecko.Api.Samples.Blazor/             WASM + AOT build proving trim safety
   ├─ CoinGecko.Api.Samples.AgentDemo/          MEAI + OpenAI + CoinGecko tools E2E
   └─ CoinGecko.Api.Samples.McpAgent/           MCP client sample
```

Spec docs under `docs/superpowers/specs/` are internal and excluded from packaged output (`<None Remove="..\..\docs\**\*.*" />` in each csproj if needed; confirmed via `dotnet pack` inspection).

---

## 4. Public API surface

### 4.1 Root client

```csharp
public interface ICoinGeckoClient
{
    ICoinsClient          Coins          { get; }
    INftsClient           Nfts           { get; }
    IExchangesClient      Exchanges      { get; }
    IDerivativesClient    Derivatives    { get; }
    ICategoriesClient     Categories     { get; }
    IAssetPlatformsClient AssetPlatforms { get; }
    ICompaniesClient      Companies      { get; }
    ISimpleClient         Simple         { get; }   // /simple/price, /simple/token_price, /simple/supported_vs_currencies
    IGlobalClient         Global         { get; }
    ISearchClient         Search         { get; }
    ITrendingClient       Trending       { get; }
    IOnchainClient        Onchain        { get; }   // GeckoTerminal: networks, pools, tokens, dexes, ohlcv, trades
    IKeyClient            Key            { get; }   // /key
    IPingClient           Ping           { get; }   // /ping
}
```

Each sub-client is a narrow interface with methods mapping 1:1 to endpoints. Coverage includes every endpoint enumerated in `docs/coingecko-api-research.md` (50+ core, 28 onchain).

### 4.2 Sub-client shape (illustrative)

```csharp
public interface ICoinsClient
{
    Task<IReadOnlyList<CoinListItem>> GetListAsync(
        bool includePlatforms = false, CancellationToken ct = default);

    Task<IReadOnlyList<CoinMarket>> GetMarketsAsync(
        string vsCurrency, CoinMarketsOptions? options = null, CancellationToken ct = default);

    Task<Coin> GetAsync(
        string id, CoinDetailOptions? options = null, CancellationToken ct = default);

    Task<CoinHistory> GetHistoryAsync(
        string id, DateOnly date, bool localization = false, CancellationToken ct = default);

    Task<MarketChart> GetMarketChartAsync(
        string id, string vsCurrency, MarketChartRange range, CancellationToken ct = default);

    Task<MarketChart> GetMarketChartRangeAsync(
        string id, string vsCurrency, DateTimeOffset from, DateTimeOffset to,
        string? interval = null, CancellationToken ct = default);

    Task<CoinTickers> GetTickersAsync(
        string id, CoinTickersOptions? options = null, CancellationToken ct = default);

    Task<Coin> GetByContractAddressAsync(
        string assetPlatformId, string contractAddress, CancellationToken ct = default);

    // Auto-pagination convenience, driven by IAsyncEnumerable; stops naturally at partial page:
    IAsyncEnumerable<CoinMarket> EnumerateMarketsAsync(
        string vsCurrency, CoinMarketsOptions? options = null, CancellationToken ct = default);
}
```

### 4.3 Options objects

Per-endpoint immutable record-like option types (not bag-of-params on the method signatures):

```csharp
public sealed record CoinMarketsOptions
{
    public IReadOnlyList<string>? Ids { get; init; }
    public string? Category { get; init; }
    public CoinMarketsOrder Order { get; init; } = CoinMarketsOrder.MarketCapDesc;
    public int PerPage { get; init; } = 100;
    public int Page { get; init; } = 1;
    public bool Sparkline { get; init; }
    public IReadOnlyList<PriceChangeWindow>? PriceChangePercentage { get; init; }
    public string? Locale { get; init; }
    public int? Precision { get; init; }
}
```

Enums preferred over magic strings for every closed-set parameter (`order`, `days`, `interval`, `localization`, etc.). `[EnumMember(Value = "market_cap_desc")]` + a generated `JsonStringEnumConverter` so wire format matches CoinGecko while API surface is strongly-typed.

### 4.4 Options / DI

```csharp
public sealed class CoinGeckoOptions
{
    public string? ApiKey              { get; set; }
    public CoinGeckoPlan Plan          { get; set; } = CoinGeckoPlan.Demo;
    public Uri?   BaseAddress          { get; set; }   // override for testing, proxies, enterprise
    public Uri?   OnchainBaseAddress   { get; set; }   // optional split (defaults to same host as BaseAddress)
    public string UserAgent            { get; set; } = "CoinGecko.Api/{version}";
    public TimeSpan Timeout            { get; set; } = TimeSpan.FromSeconds(30);
    public bool   AutoPaginate         { get; set; } = true;
    public RateLimitPolicy RateLimit   { get; set; } = RateLimitPolicy.Respect;
    public AuthenticationMode AuthMode { get; set; } = AuthenticationMode.Header;  // Header (default) | QueryString
}

public enum CoinGeckoPlan { Demo, Basic, Analyst, Lite, Pro, ProPlus, Enterprise }
// Ordered ascending by capability; the plan-enforcement handler uses ordinal
// comparison, so `[RequiresPlan(CoinGeckoPlan.Analyst)]` passes for Analyst
// and above. Base URL split is still binary (Demo → api.coingecko.com,
// everything else → pro-api.coingecko.com); the enum distinguishes tiers so
// endpoint gating and rate-limit budgeting can be plan-aware.

public enum RateLimitPolicy
{
    Respect,  // honor Retry-After, retry automatically (default)
    Throw,    // surface 429 as CoinGeckoRateLimitException immediately
    Ignore    // pass through raw to caller's layer
}

public enum AuthenticationMode { Header, QueryString }
```

DI registration:

```csharp
IHttpClientBuilder builder = services.AddCoinGeckoApi(opts =>
{
    opts.ApiKey = configuration["CoinGecko:ApiKey"];
    opts.Plan   = CoinGeckoPlan.Pro;
});

// Consumers can chain onto the returned IHttpClientBuilder:
builder.AddStandardResilienceHandler();
```

Static factory for scripts / Console / LINQPad:

```csharp
using var gecko = CoinGeckoClientFactory.Create("my-api-key", CoinGeckoPlan.Pro);
var btc = await gecko.Coins.GetAsync("bitcoin");
```

The factory wires an internal `ServiceCollection`, resolves an `ICoinGeckoClient`, and returns it. Disposal disposes the underlying scope. Thread-safe and safe to cache for the lifetime of a script.

### 4.5 Demo vs Pro endpoint gating

Methods requiring higher tiers are marked at the sub-client level with an internal `[RequiresPlan(CoinGeckoPlan.Analyst)]` attribute. The plan-enforcement handler (see §5) consults the attribute via a source-generated lookup table (no reflection on the hot path); if the caller's configured plan is below the required tier, a `CoinGeckoPlanException` is thrown *before* the HTTP request is sent. This gives a deterministic error and surfaces the upgrade path before wasting a rate-limit credit.

### 4.6 Response envelope handling

Two internal deserialization paths; both produce clean domain types that do not leak envelope shape to callers.

- **Bare**: core endpoints return raw JSON. Deserialized directly into the model type via `JsonSerializer.DeserializeAsync<T>(stream, CoinGeckoJsonContext.Default.GetTypeInfo<T>())`.
- **JsonApi**: onchain / GeckoTerminal endpoints wrap responses in `{ "data": ..., "included": [...], "meta": {...}, "links": {...} }`. An internal `JsonApiResponse<T>` is deserialized, `.Data` returned; `included` is merged into the resulting object graph when the consumer model expects related resources. Details live in `CoinGecko.Api/Serialization/JsonApi/`.

The deserializer is selected per request via an `HttpRequestOptionsKey<ResponseEnvelope>` set by the sub-client method when the request is built.

### 4.7 Exception hierarchy

```
CoinGeckoException                   abstract base; carries HttpResponseMessage + rawBody
├─ CoinGeckoRateLimitException       429; exposes Retry-After (TimeSpan?)
├─ CoinGeckoPlanException            endpoint requires higher plan; carries RequiredPlan
├─ CoinGeckoAuthException            401/403 (bad / wrong-type key)
├─ CoinGeckoNotFoundException        404
├─ CoinGeckoValidationException      400 + ValidationProblemDetails-style payload
├─ CoinGeckoServerException          5xx
└─ CoinGeckoStreamException          (WebSockets only) protocol / subscription errors
```

All exceptions are serializable, carry the request ID from telemetry for log correlation, and include the raw response body (up to a cap) to aid debugging.

---

## 5. Request pipeline

Every HTTP call flows through an `HttpClient` whose handler chain the library owns. Outer-to-inner:

| Order | Handler | Responsibility |
|---|---|---|
| 1 | `CoinGeckoAuthHandler` | Injects `x-cg-demo-api-key` or `x-cg-pro-api-key` header (or query-string if `AuthenticationMode.QueryString`); adds `User-Agent` and `Accept: application/json` |
| 2 | `CoinGeckoPlanHandler` | Reads `[RequiresPlan]` attribute from the source-generated request metadata table; short-circuits with `CoinGeckoPlanException` if the configured plan is insufficient |
| 3 | `CoinGeckoRateLimitHandler` | On `429`: reads `Retry-After` header (seconds or HTTP-date), awaits, retries up to N times. Falls back to client-side token-bucket when no header present. `CoinGeckoOptions.RateLimit` selects behavior (`Respect` / `Throw` / `Ignore`) |
| 4 | `CoinGeckoRetryHandler` | Exponential backoff with decorrelated jitter for `5xx`, `SocketException`, transient `HttpRequestException`. Bounded attempts. Never retries on caller-cancelled tokens |
| 5 | `SocketsHttpHandler` (primary) | Pooled, DNS-refresh configured, reused across requests via `IHttpClientFactory` |

Consumer-added handlers (e.g. `AddStandardResilienceHandler()`) slot *outside* position 1 — they see the authenticated request but intercept failures first, which is the correct layering when consumers want their own resilience policy.

### 5.1 Request construction

1. Sub-client method builds an `HttpRequestMessage`.
2. Path template filled via a small AOT-safe formatter: `UriTemplate.Expand("/coins/{id}", ("id", id))`.
3. Query string built by `QueryStringBuilder` — no `System.Web`, no reflection, explicit allow-list per option object.
4. Plan requirement, endpoint name (for metrics/logging), and deserializer selector attached via `HttpRequestOptionsKey<T>`.
5. `SendAsync` → pipeline.

### 5.2 Pagination

- Core endpoints are `page` / `per_page` (1-indexed).
- Callers pick per-call strategy: explicit `Page` option, or `EnumerateXxxAsync(...)` which returns `IAsyncEnumerable<T>` and internally increments `Page` until a page returns fewer rows than `PerPage`.
- Auto-pagination respects cancellation between pages and naturally inherits rate-limit backoff from the handler chain.

### 5.3 Cancellation & timeouts

- `CancellationToken` on every public async method, threaded to `HttpClient.SendAsync`.
- Per-request timeout via linked `CancellationTokenSource` (not `HttpClient.Timeout`, which fights retry semantics).
- Retry and rate-limit handlers always respect caller cancellation — never silently continue retrying on a cancelled token.

### 5.4 Observability

- Optional `ILogger<CoinGeckoClient>`; emission via `LoggerMessage` source generators (zero-alloc, AOT-safe).
- Events: `Sending` / `RateLimited` / `Retrying` / `Failed` / `Succeeded`, correlated by per-request `Guid`.
- `ActivitySource("CoinGecko.Api")` — OpenTelemetry users get traces with zero library-side OTel dependency (`ActivitySource` is in the BCL).
- Tags on activities: `http.method`, `http.status_code`, `coingecko.endpoint`, `coingecko.plan`, `coingecko.request_id`.

### 5.5 Serialization & AOT

- One root `JsonSerializerContext` per package:
  ```csharp
  [JsonSourceGenerationOptions(
      PropertyNamingPolicy   = JsonKnownNamingPolicy.SnakeCaseLower,
      NumberHandling         = JsonNumberHandling.AllowReadingFromString,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
  [JsonSerializable(typeof(Coin))]
  [JsonSerializable(typeof(CoinMarket))]
  // ... every DTO declared here
  internal sealed partial class CoinGeckoJsonContext : JsonSerializerContext { }
  ```
- Snake-case matches wire format (`market_cap_rank`). Per-property `[JsonPropertyName]` only for irregular names (`sparkline_in_7d`, etc.).
- `AllowReadingFromString`: handles CoinGecko occasionally returning numeric strings where numbers would be typed.
- Custom converters are *generated* types implementing `JsonConverter<T>`, not reflection-based. `UnixSecondsConverter : JsonConverter<DateTimeOffset>` handles timestamp fields.

---

## 6. WebSockets (`CoinGecko.Api.WebSockets`)

Per-channel typed API over `wss://stream.coingecko.com/v1`. Protocol is ActionCable-style (JSON-in-JSON envelope).

### 6.1 Public surface

```csharp
public interface ICoinGeckoStream : IAsyncDisposable
{
    StreamState State { get; }
    event EventHandler<StreamStateChangedEventArgs> StateChanged;

    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);

    Task<IAsyncDisposable> SubscribeCoinPricesAsync(       // C1
        IEnumerable<string> coinIds, Action<CoinPriceTick> onTick, CancellationToken ct = default);

    Task<IAsyncDisposable> SubscribeDexPoolsAsync(         // G1
        IEnumerable<DexPool> pools, Action<DexPoolTick> onTick, CancellationToken ct = default);

    Task<IAsyncDisposable> SubscribeDexTradesAsync(        // G2
        IEnumerable<DexPool> pools, Action<DexTrade> onTrade, CancellationToken ct = default);

    Task<IAsyncDisposable> SubscribeDexTokensAsync(        // G3
        IEnumerable<DexToken> tokens, Action<DexTokenTick> onTick, CancellationToken ct = default);

    IAsyncEnumerable<StreamMessage> RawAsync(CancellationToken ct = default);  // escape hatch
}

public enum StreamState { Disconnected, Connecting, Connected, Reconnecting, Faulted }
```

Disposing the handle returned by a `SubscribeXxxAsync` call unsubscribes that one channel; other subscriptions and the underlying connection remain open.

### 6.2 DI

```csharp
services.AddCoinGeckoStream(opts =>
{
    opts.ApiKey               = configuration["CoinGecko:ApiKey"];  // Pro key, Analyst+ plan
    opts.AutoReconnect        = true;
    opts.MaxReconnectAttempts = 10;
    opts.HeartbeatTimeout     = TimeSpan.FromSeconds(6);
});
```

### 6.3 Internals

- Built on `System.Net.WebSockets.ClientWebSocket`. No third-party WS library.
- **Custom two-layer serializer** for the ActionCable envelope: outer `{ "identifier": "{...json...}", "message": {...} }` where `identifier` is itself JSON encoded as a string. `ActionCableIdentifierConverter : JsonConverter<ActionCableIdentifier>` handles the quoted-JSON pattern; AOT-safe.
- Single dedicated long-running receive task reads frames, assembles multi-frame messages into a pooled `ArrayBufferWriter<byte>`, dispatches to subscribers via per-topic `Channel<T>`.
- Backpressure: if a subscriber is slower than the inbound rate, the topic channel drops older messages with a telemetry signal. Never blocks the receive loop.
- Heartbeat: ActionCable pings every ~3s; absence for `HeartbeatTimeout` triggers reconnect.
- Reconnect: exponential backoff (1s → 30s cap) with decorrelated jitter; active subscriptions are restored on reconnect. Disable-able.
- Subscription and connection caps from research are enforced client-side with fast-fail `CoinGeckoStreamException`.

### 6.4 State machine

```
Disconnected ─ConnectAsync─▶ Connecting ─ok─▶ Connected ─SubscribeXxx─▶ (active subscriptions)
     ▲                           │                │                            │
     │                         error            error ─▶ Reconnecting ─retry──▶ Connected
     │                           ▼                              │
     └─── Faulted (terminal, caller inspects .Exception) ◀──────┘ (after MaxReconnectAttempts)
```

### 6.5 Testing

In-repo fake WS server hosted in Kestrel speaks the ActionCable envelope. Drives the full integration suite — subscribe, unsubscribe, reconnect, heartbeat timeout, cap enforcement, dispatch ordering — with no network dependency.

---

## 7. AI Agent Hub

Two sibling packages. Both produce `IReadOnlyList<AIFunction>` so consumers can swap or mix with zero friction.

### 7.1 `CoinGecko.Api.AiAgentHub` — MEAI tool bindings backed by REST

```csharp
public static class CoinGeckoAiTools
{
    public static IReadOnlyList<AIFunction> Create(
        ICoinGeckoClient client, CoinGeckoAiToolsOptions? options = null);
}

public sealed class CoinGeckoAiToolsOptions
{
    public CoinGeckoToolSet Tools    { get; set; } = CoinGeckoToolSet.All;
    public int              MaxResults { get; set; } = 25;
    public bool             IncludeOnchainTools { get; set; } = true;
    public Func<string,bool>? ToolFilter { get; set; }
    public bool             Verbose { get; set; } = false;  // emit full models vs compact projections
}

[Flags]
public enum CoinGeckoToolSet
{
    CoinPrices  = 1 << 0,
    CoinSearch  = 1 << 1,
    MarketData  = 1 << 2,
    Trending    = 1 << 3,
    Categories  = 1 << 4,
    Nfts        = 1 << 5,
    Derivatives = 1 << 6,
    Onchain     = 1 << 7,
    All         = ~0
}
```

Usage:

```csharp
var tools = CoinGeckoAiTools.Create(gecko,
    new() { Tools = CoinGeckoToolSet.CoinPrices | CoinGeckoToolSet.Trending });

var answer = await chatClient.GetResponseAsync(
    "What's trending and what's ETH at?",
    new ChatOptions { Tools = tools });
```

Internals:

- Each tool is an `AIFunction` wrapping a specific `ICoinGeckoClient` method.
- Tool descriptions are written *for an LLM*, not for human docs — they explain when to use each tool and how it differs from neighbors. Names follow MCP conventions (`snake_case`, verb-noun).
- **AOT story**: `AIFunctionFactory.Create` currently relies on reflection. The package therefore ships an internal source generator (Roslyn) that emits concrete `AIFunction` subclasses from methods annotated `[CoinGeckoAiTool]`, used when compiled against AOT (`<IsAotCompatible>`). Non-AOT callers go through the reflection path transparently — one public entry, two code paths.
- Output shaping: list-returning tools use compact projections (e.g. `{id,name,symbol,price_usd,change_24h,market_cap}`) to conserve LLM context. `Verbose = true` returns full models. `MaxResults` caps every list.

### 7.2 `CoinGecko.Api.AiAgentHub.Mcp` — MCP client for hosted CoinGecko MCP

```csharp
public static class CoinGeckoMcp
{
    public static Task<IReadOnlyList<AIFunction>> ConnectAsync(
        string apiKey, CoinGeckoPlan plan = CoinGeckoPlan.Demo,
        CoinGeckoMcpOptions? options = null, CancellationToken ct = default);

    public static Task<IMcpClient> CreateClientAsync(
        string apiKey, CoinGeckoPlan plan = CoinGeckoPlan.Demo,
        CoinGeckoMcpOptions? options = null, CancellationToken ct = default);
}

public sealed class CoinGeckoMcpOptions
{
    public Uri?    BaseAddress { get; set; }               // for test / enterprise proxies
    public McpTransport Transport { get; set; } = McpTransport.StreamableHttp;  // or Sse
    public TimeSpan CallTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
```

Internals:

- Thin wrapper over `ModelContextProtocol.Client.McpClientFactory.CreateAsync(...)`.
- Plan picks host: Demo → `https://mcp.api.coingecko.com/mcp`, Pro → `https://mcp.pro-api.coingecko.com/mcp`. `BaseAddress` override for tests.
- Auth: `Authorization: Bearer <apiKey>` header on the MCP transport (per research).
- Default transport is Streamable HTTP with SSE fallback (matching the two modes CoinGecko exposes).
- MCP tools are wrapped as `AIFunction` via MEAI's MCP↔AIFunction adapter so §7.1 and §7.2 have identical consumer-facing type.

### 7.3 Why both exist

| Scenario | Use |
|---|---|
| Already have `ICoinGeckoClient`, no MCP deps wanted | §7.1 |
| Want CoinGecko's maintained / versioned tool definitions without mediation | §7.2 |
| Firewall blocks `mcp.coingecko.com` outbound | §7.1 |
| Agent runtime is fully MCP-native | §7.2 |

Swapping between them is a one-line change in the call site.

---

## 8. Resilience defaults

- `CoinGeckoRateLimitHandler` is the library's one non-negotiable resilience behavior. It handles the CoinGecko-specific `429 + Retry-After` protocol correctly and is always on the chain.
- `CoinGeckoRetryHandler` retries transient 5xx / socket failures with bounded attempts + decorrelated jitter. Can be disabled via options for callers who bring their own resilience stack.
- **No Polly dependency.** Consumers wanting the full Polly v8 pipeline chain `.AddStandardResilienceHandler()` on the `IHttpClientBuilder` returned by `AddCoinGeckoApi(...)`. This is the 2026 library-author best practice: sane bespoke defaults, opt-in for the heavyweight framework.

---

## 9. Testing strategy

| Project | Scope | Stack |
|---|---|---|
| `CoinGecko.Api.Tests` | Unit: sub-clients, handlers, query-string / URI template builders, pagination, cancellation, plan gating | xUnit v3 · Shouldly · NSubstitute (for `HttpMessageHandler` fakes) |
| `CoinGecko.Api.ContractTests` | Snapshot: deserialize captured response fixtures → re-serialize → diff against baseline | xUnit v3 · `Verify.Xunit` |
| `CoinGecko.Api.MockTests` | E2E: full DI → WireMock.Net HTTP fixture replaying captured payloads. Covers auth headers, plan-based base URL, rate-limit handling, JSON:API unwrap, pagination across pages, envelope selection | xUnit v3 · `WireMock.Net` |
| `CoinGecko.Api.WebSockets.Tests` | Integration: in-repo Kestrel-hosted fake ActionCable server. Covers subscribe / unsubscribe / heartbeat / reconnect / caps / dispatch order | xUnit v3 · ASP.NET Core `WebApplicationFactory` |
| `CoinGecko.Api.AiAgentHub.Tests` | Tool registration, projection shaping, MCP client init against MCP SDK's fake server | xUnit v3 |
| `CoinGecko.Api.SmokeTests` | Opt-in real-API sanity. Gated on `COINGECKO_API_KEY` env var. Excluded from CI; run locally before tagging a release | xUnit v3 |

- CI matrix: ubuntu-latest × windows-latest × macos-latest × (net8.0, net9.0). All test projects except smoke.
- Code coverage gate: 85% line coverage on `src/` (advisory, not a religion).
- Snapshot baselines committed; Verify diff-tool workflow for deliberate updates.
- `Microsoft.CodeAnalysis.PublicApiAnalyzers` validates no accidental public-API surface changes (tracked via `PublicAPI.Shipped.txt` / `.Unshipped.txt` per project).

---

## 10. Versioning & release

- **MinVer** for git-tag-based versioning. Tag format: `v<semver>-<package-shortname>`, e.g. `v1.0.0-api`, `v0.3.0-websockets`, `v0.2.0-aiagenthub`, `v0.2.0-mcp`.
- Strict SemVer 2.0. Each package versions independently.
- `release.yml` pattern-matches the tag, builds only the matching project, `dotnet pack` with SourceLink + deterministic build, pushes to nuget.org + symbols to the NuGet symbol server, creates a GitHub Release with auto-generated changelog.
- Manual approval gate (GitHub Environments) before the NuGet push — cheap insurance.
- Package metadata: `PackageId`, `Authors=msanlisavas`, `RepositoryUrl`, `RepositoryType=git`, `PackageLicenseExpression=MIT`, `PackageProjectUrl`, `PackageTags=coingecko crypto cryptocurrency api client websocket mcp ai-agent`, `PackageReadmeFile`, `PackageIcon` (from `eng/icon.png`).

---

## 11. Repo hygiene

- `Directory.Build.props`: `<LangVersion>latest</LangVersion>`, `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, `<AnalysisLevel>latest-recommended</AnalysisLevel>`, `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`, `<Deterministic>true</Deterministic>`, `<EmbedUntrackedSources>true</EmbedUntrackedSources>`, `<ContinuousIntegrationBuild Condition="'$(TF_BUILD)' == 'true' OR '$(GITHUB_ACTIONS)' == 'true'">true</ContinuousIntegrationBuild>`, `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.
- `Directory.Packages.props`: `ManagePackageVersionsCentrally=true`; every dependency version pinned here.
- `.editorconfig`: `dotnet-format`-compatible style; enforced in CI via `dotnet format --verify-no-changes`.
- `global.json`: `.NET 9.0.x`, `rollForward: latestFeature`.
- Dependabot: weekly for NuGet + GitHub Actions.
- CodeQL security scan on PRs.
- Issue templates: bug / feature / docs.
- `CODEOWNERS` pointing at `@msanlisavas`.
- `CONTRIBUTING.md` with dev setup, test matrix, PR norms.

---

## 12. Documentation

- **README** (repo root): package matrix, install, quickstart per package (REST + WS + AI + MCP), plan comparison table, compatibility matrix, contributing pointer.
- **`docs/` folder** with DocFX-generated API docs published to GitHub Pages on `main` pushes. Every public type/method has XML doc comments; `CS1591` treated as error in Release config only.
- **Sample projects** under `samples/`: Console quickstart, Blazor WASM + AOT (trim-safety proof), AgentDemo (MEAI + OpenAI + CoinGecko tools), McpAgent (MCP client).
- **No docs in `/docs/superpowers/specs/`** shipped to NuGet packages — those are internal design artifacts.

---

## 13. Deferred / open questions

1. **x402 pay-per-use endpoints** (pay-per-call USDC settlement at `pro-api.coingecko.com/api/v3/x402/...`) are documented but out of scope for v1. Evaluate for a future `CoinGecko.Api.X402` subpackage once a concrete user need exists; implementation requires a settlement signer which is a different concern from API wrapping.
2. **OpenAPI code generation** as a *build-time input* rather than runtime dependency. Kiota/NSwag could scaffold the initial DTO set from `github.com/coingecko/coingecko-api-oas`; generated output would be hand-reviewed and committed, not regenerated at runtime. Decision deferred to implementation planning — may be faster than hand-writing DTOs but the generated code quality / AOT-safety / ergonomic cost needs a spike.
3. **Strong-name signing** — currently planned *off* (matches modern .NET OSS convention, e.g. Octokit, Polly, Serilog, the Azure SDK). Revisit only if a customer explicitly needs it for a mixed-signed-assembly scenario.
4. **Native mobile targets** (MAUI, iOS, Android) — not explicit targets; works implicitly via `net8.0`/`net9.0` with AOT. Revisit if users hit MAUI-specific friction.
5. **Rate-limit policy for `IAsyncEnumerable` pagination** — current design awaits the handler chain's backoff, which is correct but can yield long pauses mid-enumeration. Consider exposing a `PaginationOptions.InterPageDelay` for callers who want explicit pacing.

---

## 14. Approval

Approved sections 1–13 during brainstorming on 2026-04-21. Implementation plan to be produced by the writing-plans skill after this spec is reviewed.
