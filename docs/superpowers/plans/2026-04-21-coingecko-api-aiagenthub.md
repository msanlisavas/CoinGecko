# CoinGecko.Api.AiAgentHub — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development.

**Goal:** Ship `CoinGecko.Api.AiAgentHub` v0.1.0-preview — a `Microsoft.Extensions.AI` tool layer that exposes CoinGecko capabilities as `AIFunction` objects consumable by any `IChatClient` (OpenAI, Anthropic, Azure OpenAI, Ollama, Gemini, Bedrock — anything with a MEAI adapter).

**Architecture:** Thin wrapper over `ICoinGeckoClient`. Each tool is a method that takes the client plus tool-specific parameters and returns a compact, LLM-friendly projection (not the full DTO). Tools are materialized via `AIFunctionFactory.Create(Delegate)` and bundled by category flags (`CoinPrices`, `CoinSearch`, `MarketData`, `Trending`, `Categories`, `Nfts`, `Derivatives`, `Onchain`). AOT limitation is inherited from `AIFunctionFactory` itself and surfaced via `[RequiresDynamicCode]` / `[RequiresUnreferencedCode]`.

**Tech Stack:** `Microsoft.Extensions.AI.Abstractions` · `CoinGecko.Api` (this repo) · xUnit v3 + Shouldly · a minimal fake `IChatClient` for invocation tests.

**Spec reference:** [`docs/superpowers/specs/2026-04-21-coingecko-wrapper-design.md`](../specs/2026-04-21-coingecko-wrapper-design.md) §7.1.

---

## Global conventions

- Working dir `D:/repos/CoinGecko/`, branch `main`.
- Every commit: `git -c commit.gpgsign=false commit -m "..." --author="msanlisavas <muratsanlisavas@gmail.com>"`. No `Co-Authored-By` trailer.
- XML `/// <summary>` on every public / protected member; `internal` exempt. CS1591 is fatal in Release.
- xUnit v3: `TestContext.Current.CancellationToken` on async calls.
- Release build 0 warnings 0 errors after every task.

## Phase index

| # | Phase | Produces |
|---|---|---|
| 0 | Scaffold | `CoinGecko.Api.AiAgentHub` project + test project. |
| 1 | Options + enum + projection DTOs | `CoinGeckoToolSet`, `CoinGeckoAiToolsOptions`, compact projections (`CoinPriceQuote`, `CoinSearchHit`, `TrendingCoinSummary`, `MarketSnapshot`, `CategorySummary`, `NftSummary`, `DerivativeSummary`, `OnchainPoolSummary`, `OnchainTokenPriceQuote`). |
| 2 | Tool method implementations | `CoinGeckoTools` static class with one public method per tool (each produces the compact projection by calling `ICoinGeckoClient` and slicing/mapping). |
| 3 | `CoinGeckoAiTools.Create` factory | Assembles `AIFunction[]` by category flags, applies `ToolFilter` and `MaxResults` caps. |
| 4 | Tests | Registration tests (correct count per flag combination), invocation tests with stub `HttpMessageHandler`, projection assertions, `MaxResults` enforcement, filter callback. |
| 5 | Sample + README + pack + tag v0.1.0-preview-aiagenthub | Runnable agent sample using `Microsoft.Extensions.AI.OpenAI` (or ollama if no key). |

Total size: ~800 lines of plan + ~1000 lines of code.

---

## Phase 0 — Scaffold

### Task 0.1: Create `CoinGecko.Api.AiAgentHub` project

**Files:**
- Create: `src/CoinGecko.Api.AiAgentHub/CoinGecko.Api.AiAgentHub.csproj`
- Create: `src/CoinGecko.Api.AiAgentHub/README.md`

- [ ] **Step 1: Scaffold**

```bash
dotnet new classlib -n CoinGecko.Api.AiAgentHub -o src/CoinGecko.Api.AiAgentHub -f net9.0 --force
rm src/CoinGecko.Api.AiAgentHub/Class1.cs
```

- [ ] **Step 2: Overwrite csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net9.0;net8.0</TargetFrameworks>
    <RootNamespace>CoinGecko.Api.AiAgentHub</RootNamespace>
    <AssemblyName>CoinGecko.Api.AiAgentHub</AssemblyName>
    <IsPackable>true</IsPackable>
    <PackageId>CoinGecko.Api.AiAgentHub</PackageId>
    <Description>Microsoft.Extensions.AI function tools backed by CoinGecko's REST API. Drop CoinGecko capabilities into any IChatClient-compatible agent (OpenAI, Anthropic, Azure OpenAI, Ollama, Gemini, Bedrock, …) in one line.</Description>
    <PackageTags>coingecko crypto cryptocurrency ai agent llm tools function-calling microsoft-extensions-ai openai anthropic</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\CoinGecko.Api\CoinGecko.Api.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.AI.Abstractions" />
  </ItemGroup>

  <ItemGroup Label="Build-only">
    <PackageReference Include="MinVer" PrivateAssets="all" />
    <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="all" />
  </ItemGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

> **AOT note:** `IsAotCompatible` and `IsTrimmable` are NOT set on this package. `AIFunctionFactory.Create(Delegate)` in `Microsoft.Extensions.AI.Abstractions` uses reflection under the hood and is annotated with `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`. Our public API propagates those attributes; users who need full AOT will wait for v0.2 (source-generated tools).

> **Version pin:** add `Microsoft.Extensions.AI.Abstractions` to `Directory.Packages.props`. Use the latest stable MEAI version. If the exact version is unknown, use `9.4.0` or the latest GA on nuget.org at execution time. Do NOT use a preview version unless that's the only option.

- [ ] **Step 3: Update `Directory.Packages.props`**

Add under the Runtime group:

```xml
<PackageVersion Include="Microsoft.Extensions.AI.Abstractions" Version="9.4.0" />
```

(Adjust the version to latest stable; pin once fixed.)

- [ ] **Step 4: Pack-time README**

`src/CoinGecko.Api.AiAgentHub/README.md`:

```markdown
# CoinGecko.Api.AiAgentHub

`Microsoft.Extensions.AI` function tools backed by the CoinGecko REST API. Give any `IChatClient`-compatible agent CoinGecko capabilities in one line.

## Install

```powershell
dotnet add package CoinGecko.Api.AiAgentHub
```

## Quickstart

```csharp
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoinGeckoApi(o => o.ApiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY"));
using var sp = services.BuildServiceProvider();
var gecko = sp.GetRequiredService<ICoinGeckoClient>();

var tools = CoinGeckoAiTools.Create(gecko, new()
{
    Tools = CoinGeckoToolSet.CoinPrices | CoinGeckoToolSet.Trending,
});

IChatClient chat = /* OpenAIClient / AnthropicClient / Ollama / Azure — any IChatClient */;
var response = await chat.GetResponseAsync(
    "What's BTC at right now and what's trending?",
    new ChatOptions { Tools = tools.Cast<AITool>().ToArray() });

Console.WriteLine(response.Text);
```

## Preview status

`Microsoft.Extensions.AI` and CoinGecko's AI Agent Hub are both evolving rapidly. This package ships in 0.x and does not declare AOT compatibility (reflection-based `AIFunctionFactory`). A v0.2 source-gen alternative is planned.

## License

MIT © msanlisavas
```

- [ ] **Step 5: Add to solution + build**

```bash
dotnet sln CoinGecko.sln add src/CoinGecko.Api.AiAgentHub/CoinGecko.Api.AiAgentHub.csproj
dotnet build src/CoinGecko.Api.AiAgentHub -c Release
```

Expected: 0 warnings, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/CoinGecko.Api.AiAgentHub/ CoinGecko.sln Directory.Packages.props
git -c commit.gpgsign=false commit -m "feat(aih): scaffold CoinGecko.Api.AiAgentHub project" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 0.2: Test project scaffold

**Files:**
- Create: `tests/CoinGecko.Api.AiAgentHub.Tests/CoinGecko.Api.AiAgentHub.Tests.csproj`
- Create: `tests/CoinGecko.Api.AiAgentHub.Tests/GlobalUsings.cs`

Follow the Phase 0.2 pattern from Plan 2 with these adjustments:
- Uses `Microsoft.NET.Sdk` (not `.Web`).
- Adds `<ProjectReference Include="..\..\src\CoinGecko.Api.AiAgentHub\CoinGecko.Api.AiAgentHub.csproj" />`.
- Also needs a `ProjectReference` to `CoinGecko.Api` (for building fake `ICoinGeckoClient` via `NSubstitute` / inline stubs).
- `<GenerateDocumentationFile>false</GenerateDocumentationFile>`.

Commit: `test(aih): scaffold CoinGecko.Api.AiAgentHub.Tests project`

---

## Phase 1 — Options, enum, projection DTOs

### Task 1.1: `CoinGeckoToolSet` enum + `CoinGeckoAiToolsOptions`

**Files:**
- Create: `src/CoinGecko.Api.AiAgentHub/CoinGeckoToolSet.cs`
- Create: `src/CoinGecko.Api.AiAgentHub/CoinGeckoAiToolsOptions.cs`
- Create: `tests/CoinGecko.Api.AiAgentHub.Tests/OptionsTests.cs`

- [ ] **Step 1: Failing test**

`tests/CoinGecko.Api.AiAgentHub.Tests/OptionsTests.cs`:

```csharp
using CoinGecko.Api.AiAgentHub;

namespace CoinGecko.Api.AiAgentHub.Tests;

public class OptionsTests
{
    [Fact]
    public void ToolSet_has_expected_flags_and_All()
    {
        CoinGeckoToolSet.CoinPrices.ShouldBe((CoinGeckoToolSet)1);
        CoinGeckoToolSet.CoinSearch.ShouldBe((CoinGeckoToolSet)2);
        CoinGeckoToolSet.MarketData.ShouldBe((CoinGeckoToolSet)4);
        CoinGeckoToolSet.Trending.ShouldBe((CoinGeckoToolSet)8);
        CoinGeckoToolSet.Categories.ShouldBe((CoinGeckoToolSet)16);
        CoinGeckoToolSet.Nfts.ShouldBe((CoinGeckoToolSet)32);
        CoinGeckoToolSet.Derivatives.ShouldBe((CoinGeckoToolSet)64);
        CoinGeckoToolSet.Onchain.ShouldBe((CoinGeckoToolSet)128);

        CoinGeckoToolSet.All.HasFlag(CoinGeckoToolSet.CoinPrices).ShouldBeTrue();
        CoinGeckoToolSet.All.HasFlag(CoinGeckoToolSet.Onchain).ShouldBeTrue();
    }

    [Fact]
    public void Options_defaults()
    {
        var o = new CoinGeckoAiToolsOptions();
        o.Tools.ShouldBe(CoinGeckoToolSet.All);
        o.MaxResults.ShouldBe(25);
        o.IncludeOnchainTools.ShouldBeTrue();
        o.ToolFilter.ShouldBeNull();
    }
}
```

- [ ] **Step 2: Run → compile fail.**

- [ ] **Step 3: Implement**

`src/CoinGecko.Api.AiAgentHub/CoinGeckoToolSet.cs`:

```csharp
namespace CoinGecko.Api.AiAgentHub;

/// <summary>Flags enum selecting which tool groups <see cref="CoinGeckoAiTools.Create"/> returns.</summary>
[Flags]
public enum CoinGeckoToolSet
{
    /// <summary>No tools.</summary>
    None        = 0,
    /// <summary>Coin price lookups (current + historical).</summary>
    CoinPrices  = 1 << 0,
    /// <summary>Coin / NFT / exchange search.</summary>
    CoinSearch  = 1 << 1,
    /// <summary>Market-cap listings (top N by market cap).</summary>
    MarketData  = 1 << 2,
    /// <summary>Trending coins / NFTs / categories.</summary>
    Trending    = 1 << 3,
    /// <summary>Coin categories.</summary>
    Categories  = 1 << 4,
    /// <summary>NFT collections.</summary>
    Nfts        = 1 << 5,
    /// <summary>Derivatives (futures + perpetuals).</summary>
    Derivatives = 1 << 6,
    /// <summary>On-chain DEX / GeckoTerminal data.</summary>
    Onchain     = 1 << 7,
    /// <summary>All tool groups.</summary>
    All         = CoinPrices | CoinSearch | MarketData | Trending | Categories | Nfts | Derivatives | Onchain,
}
```

`src/CoinGecko.Api.AiAgentHub/CoinGeckoAiToolsOptions.cs`:

```csharp
namespace CoinGecko.Api.AiAgentHub;

/// <summary>Configuration for <see cref="CoinGeckoAiTools.Create"/>.</summary>
public sealed class CoinGeckoAiToolsOptions
{
    /// <summary>Which tool groups to include.</summary>
    public CoinGeckoToolSet Tools { get; set; } = CoinGeckoToolSet.All;

    /// <summary>Cap on the number of rows list-returning tools emit (prevents LLM context overflow).</summary>
    public int MaxResults { get; set; } = 25;

    /// <summary>Include <see cref="CoinGeckoToolSet.Onchain"/> tools even when they'd be redundant with other tools. Default <c>true</c>.</summary>
    public bool IncludeOnchainTools { get; set; } = true;

    /// <summary>Optional predicate that must return true for a tool name to be included. Runs after <see cref="Tools"/> filtering.</summary>
    public Func<string, bool>? ToolFilter { get; set; }
}
```

- [ ] **Step 4: Run → pass.**

- [ ] **Step 5: Commit**

```bash
git -c commit.gpgsign=false commit -m "feat(aih): add CoinGeckoToolSet flags + CoinGeckoAiToolsOptions" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

### Task 1.2: Compact projection DTOs

Create one file per projection type under `src/CoinGecko.Api.AiAgentHub/Projections/`. Each is a **sealed record** with minimal, LLM-friendly fields — the goal is to conserve tokens. Full DTOs stay in `CoinGecko.Api`; projections here are what the LLM sees.

**Files:**
- `src/CoinGecko.Api.AiAgentHub/Projections/CoinPriceQuote.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/CoinSearchHit.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/MarketSnapshot.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/TrendingSummary.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/CategorySummary.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/NftSummary.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/DerivativeSummary.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/OnchainPoolSummary.cs`
- `src/CoinGecko.Api.AiAgentHub/Projections/OnchainTokenPriceQuote.cs`

Example — `CoinPriceQuote.cs`:

```csharp
namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact coin price snapshot for LLM consumption.</summary>
/// <param name="CoinId">CoinGecko id (e.g. <c>"bitcoin"</c>).</param>
/// <param name="Symbol">Ticker (<c>"BTC"</c>).</param>
/// <param name="Name">Display name.</param>
/// <param name="VsCurrency">Quote currency.</param>
/// <param name="Price">Current price.</param>
/// <param name="Change24hPercent">24-hour percentage change.</param>
/// <param name="MarketCap">Market capitalization in the quote currency.</param>
public sealed record CoinPriceQuote(
    string CoinId,
    string Symbol,
    string Name,
    string VsCurrency,
    decimal Price,
    decimal? Change24hPercent,
    decimal? MarketCap);
```

`MarketSnapshot.cs`:

```csharp
namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact market row for LLM consumption.</summary>
/// <param name="Rank">Market-cap rank.</param>
/// <param name="CoinId">CoinGecko id.</param>
/// <param name="Symbol">Ticker.</param>
/// <param name="Name">Display name.</param>
/// <param name="Price">Current price in USD.</param>
/// <param name="Change24hPercent">24h change percentage.</param>
/// <param name="MarketCap">Market cap in USD.</param>
/// <param name="Volume24h">24h trading volume in USD.</param>
public sealed record MarketSnapshot(
    int? Rank,
    string CoinId,
    string Symbol,
    string Name,
    decimal? Price,
    decimal? Change24hPercent,
    decimal? MarketCap,
    decimal? Volume24h);
```

Apply the same minimalist shape to the other seven projections. All properties must have XML summaries (records use `<param>` on the primary constructor — as shown above). No tests for these; coverage comes via Phase 4.

Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(aih): add compact projection records for LLM tool outputs" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 2 — Tool method implementations

### Task 2.1: `CoinGeckoTools` static class

Create `src/CoinGecko.Api.AiAgentHub/CoinGeckoTools.cs`. This class holds one **public static method per tool**. Each method:

1. Takes `ICoinGeckoClient gecko` as the first parameter.
2. Takes tool-specific parameters (strongly-typed, with XML `<param>` descriptions that read naturally to an LLM — these become the tool's parameter schema).
3. Calls the appropriate sub-client.
4. Projects the result into the compact projection type.
5. Respects `MaxResults` via a `take` parameter.
6. Returns the projection (scalar or array).

### Full method list (~15 tools)

```csharp
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub.Projections;
using CoinGecko.Api.Models;

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

        // Cross-reference names + symbols from /coins/list (cached per tool invocation — real AIFunctions have no shared cache, but
        // for v0.1 we accept the extra call; it's cheap and returns <1MB).
        // Simpler: return with CoinId only and let the LLM use GetCoinSearch to resolve if needed.

        var result = new List<CoinPriceQuote>(coinIds.Count);
        foreach (var id in coinIds)
        {
            if (!prices.TryGetValue(id, out var inner)) continue;
            if (!inner.TryGetValue(vsCurrency, out var price) || price is null) continue;
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
            if (hits.Count >= maxResults) break;
            hits.Add(new CoinSearchHit("nft", n.Id ?? "", n.Symbol ?? "", n.Name ?? "", Rank: null));
        }
        foreach (var e in r.Exchanges.Take(maxResults - hits.Count))
        {
            if (hits.Count >= maxResults) break;
            hits.Add(new CoinSearchHit("exchange", e.Id ?? "", "", e.Name ?? "", Rank: null));
        }
        foreach (var cat in r.Categories.Take(maxResults - hits.Count))
        {
            if (hits.Count >= maxResults) break;
            hits.Add(new CoinSearchHit("category", cat.Id?.ToString() ?? "", "", cat.Name ?? "", Rank: null));
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
            Coins: r.Coins.Take(maxItems).Select(c => c.Item is null ? null : new TrendingCoinSummary(
                c.Item.Id ?? "", c.Item.Symbol ?? "", c.Item.Name ?? "", c.Item.MarketCapRank)).OfType<TrendingCoinSummary>().ToArray(),
            Nfts: r.Nfts.Take(maxItems).Select(n => new TrendingNftSummary(n.Id ?? "", n.Name ?? "", n.Symbol ?? "",
                n.FloorPrice24hPercentageChange)).ToArray(),
            Categories: r.Categories.Take(maxItems).Select(c => new TrendingCategorySummary(
                c.Id?.ToString() ?? "", c.Name ?? "", c.MarketCap1hChange)).ToArray());
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
            NetworkId: /* from relationship */ null,
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
        if (prices is null) return Array.Empty<OnchainTokenPriceQuote>();
        return prices
            .Where(kvp => kvp.Value.HasValue)
            .Select(kvp => new OnchainTokenPriceQuote(
                NetworkId: network, Address: kvp.Key, PriceUsd: kvp.Value!.Value))
            .ToArray();
    }
}
```

**Note on design decisions:**

- **No paging loop in tools** — LLMs want a single call with a bounded result. Callers pass `limit` for list tools.
- **No state** — tools are stateless w.r.t. each other; each call is independent.
- **Null safety** — all projection properties are nullable where the upstream DTO's field is nullable. This preserves information loss visibility to the LLM.
- **Tool names** auto-derived from method names by MEAI (`GetCoinPrices`, `Search`, etc.). Keep them descriptive enough that the LLM picks the right one without extra hints.

Commit:

```bash
git add src/CoinGecko.Api.AiAgentHub/CoinGeckoTools.cs src/CoinGecko.Api.AiAgentHub/Projections/
git -c commit.gpgsign=false commit -m "feat(aih): add CoinGeckoTools static methods for all 8 toolsets" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 3 — `CoinGeckoAiTools.Create` factory

### Task 3.1: Factory + tool wiring

Create `src/CoinGecko.Api.AiAgentHub/CoinGeckoAiTools.cs`:

```csharp
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.AI;

namespace CoinGecko.Api.AiAgentHub;

/// <summary>Factory that produces <see cref="AIFunction"/> tools for an <see cref="ICoinGeckoClient"/>.</summary>
public static class CoinGeckoAiTools
{
    /// <summary>
    /// Build an array of <see cref="AIFunction"/> tools bound to the given <see cref="ICoinGeckoClient"/>.
    /// Pass the result as <c>ChatOptions.Tools</c> to any <see cref="IChatClient"/>.
    /// </summary>
    /// <param name="client">The underlying REST client.</param>
    /// <param name="options">Filtering and safety options.</param>
    [RequiresUnreferencedCode("AIFunctionFactory.Create uses reflection over method metadata. Not trim-safe. Use v0.2+ source-gen alternative for AOT scenarios.")]
    [RequiresDynamicCode("AIFunctionFactory.Create uses reflection. Not AOT-compatible.")]
    public static IReadOnlyList<AIFunction> Create(
        ICoinGeckoClient client,
        CoinGeckoAiToolsOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        var opts = options ?? new CoinGeckoAiToolsOptions();

        var tools = new List<AIFunction>();
        var filter = opts.ToolFilter;

        void Add(CoinGeckoToolSet flag, Delegate del, string name, string description)
        {
            if ((opts.Tools & flag) == 0) return;
            if (filter is not null && !filter(name)) return;
            var factory = AIFunctionFactory.Create(
                del,
                new AIFunctionFactoryOptions { Name = name, Description = description });
            tools.Add(factory);
        }

        // CoinPrices
        Add(CoinGeckoToolSet.CoinPrices,
            (IReadOnlyList<string> coinIds, string vsCurrency)
                => CoinGeckoTools.GetCoinPrices(client, coinIds, vsCurrency),
            name: "get_coin_prices",
            description: "Get current prices for one or more coins in a quote currency. Useful when you know coin ids like \"bitcoin\" or \"ethereum\" and want the live price.");

        // CoinSearch
        Add(CoinGeckoToolSet.CoinSearch,
            (string query, int maxResults)
                => CoinGeckoTools.Search(client, query, maxResults),
            name: "coin_search",
            description: "Search CoinGecko for coins, NFTs, exchanges, and categories by name or ticker. Use this first when the user mentions a coin by ticker or partial name.");

        // MarketData
        Add(CoinGeckoToolSet.MarketData,
            (string vsCurrency, int limit)
                => CoinGeckoTools.GetTopMarkets(client, vsCurrency, limit),
            name: "get_top_markets",
            description: "Get the top coins ranked by market capitalization. Use for overview-style questions about the market.");

        // Trending
        Add(CoinGeckoToolSet.Trending,
            (int maxItems)
                => CoinGeckoTools.GetTrending(client, maxItems),
            name: "get_trending",
            description: "Get currently trending coins, NFT collections, and categories. Use when the user asks \"what's hot\" / \"what's trending\".");

        // Categories
        Add(CoinGeckoToolSet.Categories,
            (int limit)
                => CoinGeckoTools.GetCategories(client, limit),
            name: "get_categories",
            description: "List coin sector categories (DeFi, gaming, L1, L2, etc.) with aggregate market data.");

        // Nfts
        Add(CoinGeckoToolSet.Nfts,
            (string collectionId)
                => CoinGeckoTools.GetNft(client, collectionId),
            name: "get_nft_collection",
            description: "Get detail for one NFT collection by its CoinGecko id.");

        // Derivatives
        Add(CoinGeckoToolSet.Derivatives,
            (int limit)
                => CoinGeckoTools.GetDerivatives(client, limit),
            name: "get_derivatives",
            description: "List current derivative tickers (futures, perpetuals).");

        // Onchain tools (gated by both flag AND IncludeOnchainTools)
        if (opts.IncludeOnchainTools)
        {
            Add(CoinGeckoToolSet.Onchain,
                (string query, string? network, int limit)
                    => CoinGeckoTools.SearchOnchainPools(client, query, network, limit),
                name: "search_onchain_pools",
                description: "Search DEX liquidity pools by name / symbol / contract. Filter by network id (e.g. \"eth\", \"bsc\") optionally.");

            Add(CoinGeckoToolSet.Onchain,
                (string network, IReadOnlyList<string> contractAddresses)
                    => CoinGeckoTools.GetOnchainTokenPrices(client, network, contractAddresses),
                name: "get_onchain_token_prices",
                description: "Get on-chain token prices in USD by contract address. Use for tokens not listed on CoinGecko's main aggregator.");
        }

        return tools;
    }
}
```

Notes:
- Each `Add(...)` delegate captures `client` by closure, so the resulting `AIFunction` is bound to the caller's `ICoinGeckoClient` instance.
- Tool names use `snake_case` to match MCP / OpenAI function-calling convention.
- Descriptions are for the LLM, not humans — they explain when and how to use each tool.
- `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` are propagated from `AIFunctionFactory`.

Commit:

```bash
git -c commit.gpgsign=false commit -m "feat(aih): add CoinGeckoAiTools.Create factory wiring 9 AIFunction tools" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 4 — Tests

### Task 4.1: Registration tests

`tests/CoinGecko.Api.AiAgentHub.Tests/ToolsFactoryTests.cs`:

```csharp
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using Microsoft.Extensions.AI;
using NSubstitute;

namespace CoinGecko.Api.AiAgentHub.Tests;

public class ToolsFactoryTests
{
    [Fact]
    public void Create_all_flags_returns_all_tools()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko);
        tools.Count.ShouldBe(9);
        tools.Select(t => t.Name).ShouldContain("get_coin_prices");
        tools.Select(t => t.Name).ShouldContain("get_top_markets");
        tools.Select(t => t.Name).ShouldContain("search_onchain_pools");
    }

    [Fact]
    public void Create_filtered_to_coin_prices_returns_single_tool()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.CoinPrices });
        tools.Count.ShouldBe(1);
        tools[0].Name.ShouldBe("get_coin_prices");
    }

    [Fact]
    public void Create_includeOnchainTools_false_hides_onchain_tools()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { IncludeOnchainTools = false });
        tools.Count.ShouldBe(7); // 9 - 2 onchain
        tools.Select(t => t.Name).ShouldNotContain("search_onchain_pools");
        tools.Select(t => t.Name).ShouldNotContain("get_onchain_token_prices");
    }

    [Fact]
    public void Create_custom_filter_applied()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var tools = CoinGeckoAiTools.Create(gecko, new CoinGeckoAiToolsOptions
        {
            ToolFilter = name => name.StartsWith("get_", StringComparison.Ordinal),
        });
        tools.All(t => t.Name.StartsWith("get_")).ShouldBeTrue();
        tools.Select(t => t.Name).ShouldNotContain("coin_search");
        tools.Select(t => t.Name).ShouldNotContain("search_onchain_pools");
    }
}
```

### Task 4.2: Invocation tests

`tests/CoinGecko.Api.AiAgentHub.Tests/ToolInvocationTests.cs`:

```csharp
using System.Text.Json;
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using CoinGecko.Api.AiAgentHub.Projections;
using CoinGecko.Api.Models;
using CoinGecko.Api.Resources;
using Microsoft.Extensions.AI;
using NSubstitute;

namespace CoinGecko.Api.AiAgentHub.Tests;

public class ToolInvocationTests
{
    [Fact]
    public async Task get_coin_prices_invokes_simple_client_and_projects()
    {
        var gecko = Substitute.For<ICoinGeckoClient>();
        var simple = Substitute.For<ISimpleClient>();
        gecko.Simple.Returns(simple);

        var priceMap = new Dictionary<string, IReadOnlyDictionary<string, decimal?>>
        {
            ["bitcoin"] = new Dictionary<string, decimal?>
            {
                ["usd"] = 42000m, ["usd_24h_change"] = 1.23m, ["usd_market_cap"] = 800_000_000_000m,
            },
        };
        simple.GetPriceAsync(Arg.Any<SimplePriceOptions>(), TestContext.Current.CancellationToken)
              .Returns((IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal?>>)priceMap);

        var tools = CoinGeckoAiTools.Create(gecko,
            new CoinGeckoAiToolsOptions { Tools = CoinGeckoToolSet.CoinPrices });
        var tool = tools.Single();

        var args = new Dictionary<string, object?>
        {
            ["coinIds"] = new[] { "bitcoin" },
            ["vsCurrency"] = "usd",
        };
        var result = await tool.InvokeAsync(new AIFunctionArguments(args), TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        // result is the projection array; MEAI wraps it as JSON. Deserialize to verify.
        var json = JsonSerializer.Serialize(result);
        json.ShouldContain("bitcoin");
        json.ShouldContain("42000");
    }
}
```

> **Note:** The exact `AIFunctionArguments` API shape depends on the MEAI version. If `tool.InvokeAsync(IEnumerable<KeyValuePair<string, object?>>, CancellationToken)` is the signature available, use that instead. Verify by reading the MEAI SDK docs / XML at execution time.

### Task 4.3: Commit tests

After both test files are in place, all tests green, Release build green:

```bash
git add tests/CoinGecko.Api.AiAgentHub.Tests/
git -c commit.gpgsign=false commit -m "test(aih): factory registration + invocation tests" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

---

## Phase 5 — Sample, README, pack, tag

### Task 5.1: Agent sample

`samples/CoinGecko.Api.Samples.AgentDemo/CoinGecko.Api.Samples.AgentDemo.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <RootNamespace>CoinGecko.Api.Samples.AgentDemo</RootNamespace>
    <IsPackable>false</IsPackable>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\CoinGecko.Api.AiAgentHub\CoinGecko.Api.AiAgentHub.csproj" />
  </ItemGroup>

</Project>
```

For a real-world sample that hits an LLM, we'd add `Microsoft.Extensions.AI.OpenAI` or similar — but that requires a billable API key, so keep the sample **non-LLM**. It just shows how to construct the tools and print their names + parameter schemas:

`samples/CoinGecko.Api.Samples.AgentDemo/Program.cs`:

```csharp
using CoinGecko.Api;
using CoinGecko.Api.AiAgentHub;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoinGeckoApi(o => o.ApiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY"));
using var sp = services.BuildServiceProvider();
var gecko = sp.GetRequiredService<ICoinGeckoClient>();

var tools = CoinGeckoAiTools.Create(gecko, new()
{
    Tools = CoinGeckoToolSet.All,
    MaxResults = 25,
});

Console.WriteLine($"Registered {tools.Count} tools:\n");
foreach (var t in tools)
{
    Console.WriteLine($"  • {t.Name}");
    Console.WriteLine($"    {t.Description}");
    Console.WriteLine();
}

// Invoke one tool directly as a smoke test (no LLM — just verifies the wiring):
var search = tools.First(t => t.Name == "coin_search");
var result = await search.InvokeAsync(new Microsoft.Extensions.AI.AIFunctionArguments(
    new Dictionary<string, object?> { ["query"] = "bitcoin", ["maxResults"] = 3 }));
Console.WriteLine($"Sample invocation result: {System.Text.Json.JsonSerializer.Serialize(result)}");
```

Add to solution. Verify build.

### Task 5.2: Update repo README

Update `README.md` package table: `.AiAgentHub` status `Planned` → `v0.1.0-preview`. Add a "AI tools" section demonstrating the quickstart.

### Task 5.3: Pack smoke check + tag

```bash
dotnet pack src/CoinGecko.Api.AiAgentHub/CoinGecko.Api.AiAgentHub.csproj -c Release /p:Version=0.1.0-preview -o artifacts/aih-preview

# inspect:
unzip -o artifacts/aih-preview/CoinGecko.Api.AiAgentHub.0.1.0-preview.nupkg -d artifacts/aih-preview/inspect
ls artifacts/aih-preview/inspect/lib/

# cleanup:
rm -rf artifacts/aih-preview
```

Verify: `lib/net9.0/CoinGecko.Api.AiAgentHub.dll`, `lib/net8.0/...`, `.nuspec` has correct metadata + dependencies on `CoinGecko.Api` + `Microsoft.Extensions.AI.Abstractions`.

Tag (local, do NOT push):

```bash
git tag -a v0.1.0-preview-aiagenthub -m "CoinGecko.Api.AiAgentHub 0.1.0-preview — MEAI function tools (9 tools, 8 categories)"
```

### Task 5.4: Final commit

```bash
git add samples/CoinGecko.Api.Samples.AgentDemo/ CoinGecko.sln README.md
git -c commit.gpgsign=false commit -m "docs: AiAgentHub sample + README update + v0.1.0-preview" --author="msanlisavas <muratsanlisavas@gmail.com>"
```

Final verification:
- `dotnet build CoinGecko.sln -c Release` — 0 warnings, 0 errors
- `dotnet test CoinGecko.sln -c Release` — 186 (prior) + ~7 new = ~193 per TFM
- Tags: `v0.1.0-api`, `v0.1.0-preview-websockets`, `v0.1.0-preview-aiagenthub`

---

## Appendix A — AOT / trimming story

`AIFunctionFactory.Create(Delegate)` fundamentally requires reflection metadata for parameter serialization. For v0.1-preview, we accept this and propagate `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` on our public API. Users who need AOT will see build-time warnings and can either:

1. Wait for v0.2 which ships a `[CoinGeckoAiTool]` source generator that emits concrete `AIFunction` subclasses at compile time.
2. Handwrite `AIFunction` subclasses themselves using the projection types in `src/.../Projections/`.

## Appendix B — Deferred to v0.2

- Source generator for AOT-safe tool registration.
- Structured error responses from tool invocations (today, exceptions propagate; LLMs would benefit from typed error shapes).
- Tool versioning metadata (`schemaVersion` on each projection).
- Pagination helpers inside tool methods (`continuation_token`-style for >100-item lists).
- Cross-tool coin id → symbol/name cache (cheap `coin_list` snapshot) to enrich `GetCoinPrices` output.

## Appendix C — Commit style

`feat(aih): …`, `fix(aih): …`, `test(aih): …`, `docs: …`. Subject ≤72 chars. No AI co-author trailers.
