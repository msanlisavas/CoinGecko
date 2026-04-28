# Changelog

All notable changes to the packages in this repository are tracked here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and packages follow [Semantic Versioning](https://semver.org/) — note that `0.x` releases may include breaking changes in a minor bump.

Each package is versioned independently. Tag patterns:

- `v{semver}-api` → `CoinGecko.Api`
- `v{semver}-preview-websockets` → `CoinGecko.Api.WebSockets`
- `v{semver}-preview-aiagenthub` → `CoinGecko.Api.AiAgentHub`
- `v{semver}-preview-mcp` → `CoinGecko.Api.AiAgentHub.Mcp`

---

## CoinGecko.Api

### 0.2.0 — 2026-04-28

Tracks the CoinGecko API additions announced in the April 2026 newsletter.

#### Added
- **Crypto News API** — new `INewsClient` exposing `GET /news` with full query options (`page`, `per_page`, `coin_id`, `language`, `type`). Plan-gated to Analyst+ via `[RequiresPlan]`. Wired into `ICoinGeckoClient.News`.
- **Outstanding Token Value (OTV)** — `outstanding_supply` and `outstanding_token_value_usd` on `CoinMarketData` and `OnchainTokenAttributes`.
- **`gt_verified` flag** — on `OnchainTokenAttributes` and `PoolAttributes` to surface GeckoTerminal verification.
- **`OnchainTopHoldersOptions`** — new options record exposing `Holders` (count or `"max"`) and `IncludePnlDetails`.
- **Strongly typed `OnchainTopHolder`** — now carries `Rank`, `Address`, `Label`, `Amount`, `Percentage`, `Value`, plus `AverageBuyPriceUsd`, `TotalBuyCount`, `TotalSellCount`, `RealizedPnlUsd`, `RealizedPnlPercentage`, `UnrealizedPnlUsd`, `UnrealizedPnlPercentage`, and `ExplorerUrl`.
- **`OnchainTopHolders` envelope** — new wrapper type mapping the actual `data.attributes.{last_updated_at, holders[]}` shape returned by the endpoint.
- **`CoinGeckoPaidMockFixture`** (test infra) — Pro-plan fixture for WireMock tests of plan-gated endpoints.

#### Changed (breaking)
- `IOnchainClient.GetTopHoldersAsync` return type changed from `Task<OnchainTopHolder[]>` to `Task<OnchainTopHolders>`. The previous shape was a placeholder that did not match the live API response (which returns a single envelope object containing the holder array under `attributes.holders`, not a top-level array). Migration: read `result.Attributes?.Holders` instead of indexing the result directly. The method also gains an optional `OnchainTopHoldersOptions? options` parameter (defaults to `null`).
- `OnchainTopHolder.Attributes` (previously `JsonObject?`) is removed. The fields formerly nested under `attributes` are now first-class properties on `OnchainTopHolder` itself.

#### Notes
- **Hourly market-chart intervals** were already supported via the existing `string? interval` parameter on the Coins market-chart methods (`"hourly"` is now a valid value across all plans).
- **Multi-currency WebSocket** (CGSimplePrice `vs_currencies`) was already wired on `SubscribeCoinPricesAsync(coinIds, vsCurrencies, …)` — no SDK change required.

### 0.1.1 — 2026-04-21
- Live-Demo compatibility fixes: tolerant decimal parsing, ISO-8601 date handling, string category ids, float-typed counts, volume chart edge cases, error-handler regression. See [release notes](https://github.com/msanlisavas/CoinGecko/releases/tag/v0.1.1-api).

### 0.1.0 — 2026-04-21
- Initial release. 14 sub-clients across Coins, NFTs, Exchanges, Derivatives, Categories, AssetPlatforms, Companies, Simple, Global, Search, Trending, Onchain (GeckoTerminal), Key, Ping. Plan-aware HTTP pipeline with `[RequiresPlan]` gating, source-generated JSON, AOT/trim-safe, retry + rate-limit handlers.

---

## CoinGecko.Api.AiAgentHub

### 0.2.0-preview — 2026-04-28

#### Added
- **`get_crypto_news` tool** — `CoinGeckoTools.GetCryptoNews(client, coinId?, language?, maxArticles)` returns a lean `NewsSummary[]` projection (title/url/author/postedAt/sourceName/type/relatedCoinIds). Registered under the new `CoinGeckoToolSet.News` flag.
- **`get_top_token_holders` tool** — `CoinGeckoTools.GetTopTokenHolders(client, network, tokenAddress, topN, includePnl)` returns a `TopHolderSummary[]` projection with rank/address/balance/percentage/USD value, plus average buy price and realized/unrealized PnL when `includePnl=true`. Registered under the new `CoinGeckoToolSet.TopHolders` flag and gated by `IncludeOnchainTools`.
- **Projections** — `Projections/NewsSummary.cs` and `Projections/TopHolderSummary.cs` records for LLM consumption.

#### Changed
- `CoinGeckoToolSet.All` now includes `News | TopHolders` in addition to the existing flags. The default tool count returned by `CoinGeckoAiTools.Create` rose from 9 to 11 (8 with `IncludeOnchainTools = false`).

### 0.1.1-preview — 2026-04-21
- Live-Demo compatibility (consumes `CoinGecko.Api 0.1.1`).

### 0.1.0-preview — 2026-04-21
- Initial release. `Microsoft.Extensions.AI` `AIFunction` tools backed by the REST client. 9 tools covering coin prices, search, top markets, trending, categories, NFTs, derivatives, and on-chain pools/token prices.

---

## CoinGecko.Api.WebSockets

### 0.1.1-preview — 2026-04-21
- Bundled with the `CoinGecko.Api 0.1.1` patch release. No streaming-side changes.

### 0.1.0-preview — 2026-04-21
- Initial release. WebSocket beta client with four typed channels: `CGSimplePrice` (C1), `OnchainSimpleTokenPrice` (G1), `OnchainTrade` (G2), `OnchainOHLCV` (G3). Auto-reconnect, heartbeat, restorable subscriptions.

---

## CoinGecko.Api.AiAgentHub.Mcp

### 0.1.1-preview — 2026-04-21
- Bundled with the `CoinGecko.Api 0.1.1` patch release. No MCP-client changes.

### 0.1.0-preview — 2026-04-21
- Initial release. Client for CoinGecko's hosted MCP server (`mcp.api.coingecko.com` / `mcp.pro-api.coingecko.com`). Wraps remote tools as `Microsoft.Extensions.AI.AIFunction` instances.
