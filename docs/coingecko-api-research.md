# CoinGecko API Research — Library Design Reference

_Compiled 2026-04-21 for the production-grade C# CoinGecko wrapper targeting NuGet._

Primary sources:
- Docs index: <https://docs.coingecko.com/llms.txt>
- Pro REST reference: <https://docs.coingecko.com/reference/introduction>
- Demo REST reference: <https://docs.coingecko.com/v3.0.1/reference/introduction>
- OpenAPI repo (authoritative): <https://github.com/coingecko/coingecko-api-oas>
- MCP docs: <https://docs.coingecko.com/reference/mcp-server> and <https://mcp.api.coingecko.com/>
- WebSocket docs: <https://docs.coingecko.com/websocket/cgsimpleprice>
- Pricing: <https://www.coingecko.com/en/api/pricing>

Pages that 404'd while researching (noted so future updates can retry): `/reference/ai-agent-hub`, `/reference/common-errors-and-troubleshooting`, `/reference/websocket-connect`, `/reference/cg-simple-price`, `/reference/onchain-simple-token-price`, `/reference/wss-onchain-simple-token-price`, `/reference/wss-onchain-ohlcv`, `/reference/clients`, `/reference/global`, `/reference/best-practices`, `/reference/asyncapi`, `/reference/x402`, `/reference/x402-overview`. The docs site has slugged variants (e.g. `/websocket/cgsimpleprice` works, `/reference/cg-simple-price` does not); the C# library should not hard-code doc URLs.

---

## 1. Overview / Auth / Rate Limits / Errors

### 1.1 Base URLs

| Tier                | Base URL                                                       | Notes                                                            |
| ------------------- | -------------------------------------------------------------- | ---------------------------------------------------------------- |
| Public / Demo       | `https://api.coingecko.com/api/v3/`                            | Free plan, Demo API key required (since mid-2024)                |
| Pro (all paid tiers)| `https://pro-api.coingecko.com/api/v3/`                        | Analyst, Lite, Pro, Pro+, Enterprise                             |
| Onchain (Pro)       | `https://pro-api.coingecko.com/api/v3/onchain/…`               | GeckoTerminal endpoints, same auth header                        |
| Onchain (Demo)      | `https://api.coingecko.com/api/v3/onchain/…`                   | Limited subset                                                   |
| x402 Pay-per-use    | `https://pro-api.coingecko.com/api/v3/x402/onchain/…`          | No API key; HTTP 402 + USDC settlement                           |
| WebSocket (Beta)    | `wss://stream.coingecko.com/v1`                                | Pro key, Analyst+ plan                                           |
| MCP (public)        | `https://mcp.api.coingecko.com/mcp` (HTTP streaming) / `.../sse` (SSE) | Keyless, shared rate limits                              |
| MCP (pro / BYOK)    | `https://mcp.pro-api.coingecko.com/mcp` / `.../sse`            | Browser OAuth or env-var API key                                 |

Source: <https://docs.coingecko.com/reference/setting-up-your-api-key>, <https://docs.coingecko.com/reference/authentication>, <https://docs.coingecko.com/reference/mcp-server>.

### 1.2 Authentication

Two independent key types, passed on **every** request:

| Plan type    | Header name           | Query-param fallback   |
| ------------ | --------------------- | ---------------------- |
| Demo / Public| `x-cg-demo-api-key`   | `x_cg_demo_api_key`    |
| Pro (any paid)| `x-cg-pro-api-key`   | `x_cg_pro_api_key`     |

- Header is strongly preferred over query param ("better security"). The C# client should default to header and only expose query-param auth as an opt-in for edge cases (e.g. caching proxies that require keys in the URL).
- Keys are NOT interchangeable between the two base URLs — a Demo key sent to `pro-api.coingecko.com` returns `10011`, and vice versa returns `10010`.
- `1 successful call = 1 credit`. 4xx/5xx don't burn credits but still count against per-minute rate limit.
- Extra endpoint: `GET /key` returns your plan, rate limit, and remaining monthly credits (only available on Pro).

### 1.3 Required / Recommended Headers (what the C# `HttpClient` should send)

| Header              | Purpose                                                                 |
| ------------------- | ----------------------------------------------------------------------- |
| `x-cg-pro-api-key` *or* `x-cg-demo-api-key` | Auth. Mutually exclusive per configured base URL.    |
| `Accept: application/json`                  | Defensive; API returns JSON by default.             |
| `User-Agent`        | CoinGecko occasionally fingerprints and CDN (Cloudflare, error 1020) blocks empty/suspicious UAs. Set something like `MySdkName/1.0 (+https://github.com/…)`. |
| `Accept-Encoding: gzip, deflate, br` | Large endpoints (`/coins/markets`, `/exchanges/{id}/tickers`) return large JSON. `HttpClient` should enable automatic decompression. |

CoinGecko does **not** document custom rate-limit response headers (no `X-RateLimit-*`); 429 is the sole signal. The library should rely on 429 + Retry-After (when present) and the `/key` polling endpoint for proactive throttling.

### 1.4 Rate Limits & Plans

From <https://www.coingecko.com/en/api/pricing> and `/key` response semantics:

| Plan       | Monthly (USD) | Annual (USD) | Calls/min (rate limit) | Monthly credits  | Historical depth  | API keys | Notes |
| ---------- | ------------- | ------------ | ---------------------- | ---------------- | ----------------- | -------- | ----- |
| Demo       | Free          | Free         | ~30 (varies w/ traffic)| 10,000           | Past 1 year       | 1        | Attribution required. Uses `api.coingecko.com`. |
| Basic      | $35           | $348         | 250                    | 100,000          | 2 years           | 1        | 50+ endpoints, commercial use |
| Analyst    | $129          | $1,238       | 500                    | 500,000          | 2013 → present    | —        | 70+ endpoints. Unlocks WebSocket beta & exclusive endpoints (top gainers/losers, NFT historical chart, pool megafilter, etc.) |
| Lite       | $499          | $4,790       | 500                    | 2,000,000        | 2013 → present    | 10       | 5 team members |
| Pro / Pro+ | $499+         | $4,790+      | 500+ (tiered)          | 2M / 5M / 8M / 10M / 15M | 2013 → present | 10+   | Flexible credit tiers |
| Enterprise | Custom        | Custom       | Custom (thousands/min) | Custom           | 2013 → present    | 25       | 80+ endpoints, 99.9% SLA, Slack support, 20 team members |

Overage (Basic → Lite): $0.0005 per call. The client should treat `current_remaining_monthly_calls == 0` plus 429s as a billing event.

### 1.5 HTTP Error Codes (from <https://docs.coingecko.com/reference/common-errors-rate-limit>)

| Code   | Meaning                                                        | C# handling |
| ------ | -------------------------------------------------------------- | ----------- |
| 400    | Malformed request                                              | Throw `CoinGeckoBadRequestException` |
| 401    | Missing / invalid auth                                         | Throw `CoinGeckoAuthenticationException` |
| 403    | Endpoint blocked for this plan or IP                           | Throw `CoinGeckoForbiddenException` |
| 408    | Server didn't receive complete request in time                 | Retry with backoff |
| 429    | Rate limit exceeded                                            | Respect `Retry-After` if present, otherwise exponential backoff; throw `CoinGeckoRateLimitException` after N retries |
| 500    | Server error                                                   | Retry with backoff |
| 503    | Service unavailable (check status.coingecko.com)               | Retry with backoff |
| 1020   | Cloudflare firewall rule (typically UA/origin issue)           | Surface as `CoinGeckoCdnBlockedException`, do NOT auto-retry |
| 10002  | Missing API key                                                | Configuration error |
| 10005  | Pro-only endpoint, caller is on Demo                           | Throw `CoinGeckoPlanException` |
| 10010  | Wrong Pro API key                                              | Configuration error |
| 10011  | Wrong Demo API key                                             | Configuration error |

Error body shape observed: `{ "status": { "error_code": <int>, "error_message": "..." } }` — but CoinGecko does not formally document a canonical envelope; parser should be tolerant of both `{ status: {...} }` and plain `{ error: "..." }` forms.

---

## 2. AI Agent Hub (MCP Server + x402 + Prompts)

"AI Agent Hub" is CoinGecko's umbrella for everything LLM-adjacent. It is **not** a single endpoint — it's a collection of integrations:

1. **MCP Server** (Beta) — the primary product. Official Model Context Protocol server exposing the REST catalogue as MCP tools.
2. **x402 endpoints** — HTTP 402 + USDC micropayments for keyless, per-request access.
3. **AI prompt libraries** — prebuilt prompts for Python/TypeScript SDKs.
4. **Agent SKILL** — a Claude Code skill for wiring CoinGecko into coding agents.
5. **CoinGecko CLI** — a Go-based terminal REPL for crypto data.
6. **IDE integrations** — Cursor / Claude Code / Claude Desktop / Gemini CLI / ChatGPT preset configs.

Source: <https://docs.coingecko.com/llms.txt>, <https://docs.coingecko.com/reference/mcp-server>, <https://www.coingecko.com/learn/x402-pay-per-use-crypto-api>.

### 2.1 MCP Server — deep dive

Current status: **Public Beta**.

| Server              | URL (HTTP streaming)                          | URL (SSE fallback)                            | Auth |
| ------------------- | --------------------------------------------- | --------------------------------------------- | ---- |
| Public / keyless    | `https://mcp.api.coingecko.com/mcp`           | `https://mcp.api.coingecko.com/sse`           | None; shared rate limit |
| Pro (BYOK)          | `https://mcp.pro-api.coingecko.com/mcp`       | `https://mcp.pro-api.coingecko.com/sse`       | Browser OAuth flow OR env `COINGECKO_PRO_API_KEY` |
| Local (npm)         | `npx -y @coingecko/coingecko-mcp`             | stdio                                         | Env vars: `COINGECKO_PRO_API_KEY` or `COINGECKO_DEMO_API_KEY`, `COINGECKO_ENVIRONMENT=pro|demo` |

Protocol: MCP (Model Context Protocol). Two transports auto-negotiated:
- **HTTP streaming** (preferred, bidirectional long-poll).
- **SSE** (server-sent events, legacy fallback).

Tool catalogue: **76+ tools** (as of 2026-Q1), effectively a 1:1 wrapping of the REST endpoints. Tool discovery modes:
- **Static** (default): client receives full tool list up front.
- **Dynamic**: client queries the server with keyword search to receive a subset. Faster cold start, slower runtime.

Keyless tier: Demo-equivalent rate limits (~30/min, 10k monthly credits, 1-year history).  
Pro tier: plan-dependent (500+/min).

**Client config example** (Claude Desktop / Cursor / Windsurf JSON):
```json
{
  "mcpServers": {
    "coingecko_mcp": {
      "command": "npx",
      "args": ["mcp-remote", "https://mcp.api.coingecko.com/mcp"]
    }
  }
}
```

**How the C# library should expose this**: do NOT try to wrap MCP inside the REST client. MCP is a separate transport (JSON-RPC 2.0 over HTTP-streaming/SSE/stdio). For a NuGet package:

Option A (recommended): ship only the REST + WebSocket client; let MCP consumers use the official `@coingecko/coingecko-mcp` npm package via their agent framework. Document this in the README.

Option B: add a secondary `CoinGecko.Mcp` package that wraps either `ModelContextProtocol.NET` or the `Anthropic.SDK` MCP client. Useful if the consumer wants to build an AI agent entirely in C# that talks to `mcp.api.coingecko.com`. This is additive and should not pollute the core REST package.

Tool-calling schema: MCP tools accept the same parameter names as the REST query params (e.g. `get_coins_markets(vs_currency="usd", per_page=50)`). If the library offers "prompt templates" (Option B), each REST method's C# `Request` object maps 1:1 to an MCP tool input schema, so codegen from the OpenAPI spec drives both.

### 2.2 x402 endpoints

Protocol: open standard from Coinbase reviving HTTP 402 for per-request USDC payments.

- Price: **$0.01 USDC per request**, on Base chain.
- No API key; no account. Just a wallet.
- Request flow:
  1. Client hits e.g. `GET https://pro-api.coingecko.com/api/v3/x402/onchain/simple/networks/base/token_price/{addresses}`.
  2. Server returns `402 Payment Required` with `payment-requirements` response headers/body describing amount, recipient, chain, token.
  3. Client wallet signs an EIP-3009 `transferWithAuthorization` over USDC.
  4. Client resends the same request with an `X-PAYMENT` header carrying the signature.
  5. Server verifies, settles on-chain, and returns 200 with the data.
- Known x402 paths (experimental; CoinGecko recommends standard subscription for production):
  - `/x402/onchain/simple/networks/base/token_price/{addresses}`
  - `/x402/onchain/search/pools`
  - `/x402/onchain/networks/base/trending_pools`
  - More likely to be added — enumerate dynamically.

C# implementation note: this is out of scope for a v1 NuGet wrapper unless the consumer specifically asks for agent-driven micropayments. Put it behind an optional extension package (`CoinGecko.X402`) that takes an `IEthereumSigner` abstraction so the core library stays wallet-free.

---

## 3. Pro API — Complete Endpoint Inventory

Source: <https://raw.githubusercontent.com/coingecko/coingecko-api-oas/refs/heads/main/coingecko-pro.json> + <https://docs.coingecko.com/llms.txt>. Tier column: **Demo** = also on free plan, **Pro** = any paid plan (Basic+), **Analyst+** = Analyst or higher (💼 / 🔥 markers in the OAS). `/onchain/*` are GeckoTerminal endpoints and listed separately in §3.2.

### 3.1 Core (CoinGecko)

| Path | Verb | Tier | Description | Key params |
| --- | --- | --- | --- | --- |
| `/ping` | GET | Demo | Health check | — |
| `/key` | GET | Pro | Current API usage, rate limit, credits remaining | — |
| `/simple/price` | GET | Demo | Price by coin IDs / symbols / names | `ids`, `names`, `symbols`, `vs_currencies` (plural!), `include_tokens`, `include_market_cap`, `include_24hr_vol`, `include_24hr_change`, `include_last_updated_at`, `precision` |
| `/simple/token_price/{id}` | GET | Demo | Price by platform + contract addresses | `id` = asset platform (e.g. `ethereum`), `contract_addresses`, `vs_currencies`, include flags, `precision` |
| `/simple/supported_vs_currencies` | GET | Demo | List of supported quote currencies | — |
| `/search` | GET | Demo | Search coins/categories/markets/NFTs | `query` |
| `/search/trending` | GET | Demo | Trending (coins, NFTs, categories) | `show_max` (enum, CSV) |
| `/coins/list` | GET | Demo | All coins ID map | `include_platform`, `status` |
| `/coins/markets` | GET | Demo | Paginated market data | `vs_currency` (singular), `ids`, `names`, `symbols`, `category`, `order`, `per_page` (1-250, default 100), `page`, `sparkline`, `price_change_percentage` (CSV of `1h,24h,7d,14d,30d,200d,1y`), `locale`, `precision` |
| `/coins/list/new` | GET | Pro | Last 200 listed coins | — |
| `/coins/top_gainers_losers` | GET | Analyst+ | Top 30 gainers/losers | `vs_currency`, `duration`, `top_coins` |
| `/coins/{id}` | GET | Demo | Full coin metadata + market + tickers | `localization`, `tickers`, `market_data`, `community_data`, `developer_data`, `sparkline`, `include_categories_details`, `dex_pair_format` |
| `/coins/{id}/tickers` | GET | Demo | Tickers across exchanges | `exchange_ids`, `include_exchange_logo`, `page`, `order`, `depth`, `dex_pair_format` |
| `/coins/{id}/history` | GET | Demo | Historical snapshot on a date | `date` (dd-mm-yyyy), `localization` |
| `/coins/{id}/market_chart` | GET | Demo | Time series (prices, market_caps, total_volumes) | `vs_currency`, `days` (int or `max`), `interval` (`5m`, `hourly`, `daily`), `precision` |
| `/coins/{id}/market_chart/range` | GET | Demo | Same, for [from, to] unix seconds | `vs_currency`, `from`, `to`, `precision` |
| `/coins/{id}/ohlc` | GET | Demo | OHLC candles | `vs_currency`, `days`, `precision`, `interval` |
| `/coins/{id}/ohlc/range` | GET | Analyst+ | OHLC for range | `vs_currency`, `from`, `to`, `interval`, `precision` |
| `/coins/{id}/circulating_supply_chart` | GET | Analyst+ | Circulating supply history | `days`, `interval` |
| `/coins/{id}/circulating_supply_chart/range` | GET | Analyst+ | Same for range | `from`, `to` |
| `/coins/{id}/total_supply_chart` | GET | Analyst+ | Total supply history | `days`, `interval` |
| `/coins/{id}/total_supply_chart/range` | GET | Analyst+ | Same for range | `from`, `to` |
| `/coins/{id}/contract/{contract_address}` | GET | Demo | Coin metadata by on-chain contract | — |
| `/coins/{id}/contract/{contract_address}/market_chart` | GET | Demo | Chart by contract | `vs_currency`, `days`, `precision` |
| `/coins/{id}/contract/{contract_address}/market_chart/range` | GET | Demo | Chart range by contract | `vs_currency`, `from`, `to`, `precision` |
| `/coins/categories/list` | GET | Demo | Category ID map | — |
| `/coins/categories` | GET | Demo | Categories with market data | `order` |
| `/asset_platforms` | GET | Demo | All asset platforms (blockchains) | `filter` |
| `/token_lists/{asset_platform_id}/all.json` | GET | Pro | Uniswap-style token list | — |
| `/exchanges` | GET | Demo | Exchanges + summary data | `per_page`, `page` |
| `/exchanges/list` | GET | Demo | Exchange ID map | `status` |
| `/exchanges/{id}` | GET | Demo | Exchange details + top 100 tickers | — |
| `/exchanges/{id}/tickers` | GET | Demo | Tickers by exchange | `coin_ids`, `include_exchange_logo`, `page`, `depth`, `order`, `dex_pair_format` |
| `/exchanges/{id}/volume_chart` | GET | Demo | Historical BTC volume | `days` |
| `/exchanges/{id}/volume_chart/range` | GET | Pro | Same for range | `from`, `to` |
| `/derivatives` | GET | Demo | Derivatives tickers | — |
| `/derivatives/exchanges` | GET | Demo | Derivatives exchanges list | `order`, `per_page`, `page` |
| `/derivatives/exchanges/{id}` | GET | Demo | Derivatives exchange by ID | `include_tickers` |
| `/derivatives/exchanges/list` | GET | Demo | Derivatives exchanges ID map | — |
| `/nfts/list` | GET | Demo | NFT collections ID map | `order`, `per_page`, `page` |
| `/nfts/list_with_market_data` | GET | Pro | NFTs with market data | `order`, `per_page`, `page`, `asset_platform_id` |
| `/nfts/{id}` | GET | Demo | NFT collection by ID | — |
| `/nfts/{id}/market_chart` | GET | Pro | Floor price / market cap / volume history | `days` |
| `/nfts/{id}/tickers` | GET | Pro | Marketplace tickers | — |
| `/nfts/{asset_platform_id}/contract/{contract_address}` | GET | Demo | NFT by contract address | — |
| `/nfts/{asset_platform_id}/contract/{contract_address}/market_chart` | GET | Pro | NFT history by contract | `days` |
| `/nfts/markets` | GET | Pro | NFTs with market data (paginated) | `asset_platform_id`, `order`, `per_page`, `page` |
| `/exchange_rates` | GET | Demo | BTC exchange rates (to fiat + crypto) | — |
| `/global` | GET | Demo | Global market snapshot | — |
| `/global/decentralized_finance_defi` | GET | Demo | Global DeFi snapshot | — |
| `/global/market_cap_chart` | GET | Pro | Global mcap history | `days`, `vs_currency` |
| `/entities/list` | GET | Pro | Treasury entities (companies/governments) ID map | `country`, `type`, `order`, `per_page`, `page` |
| `/{entity}/public_treasury/{coin_id}` | GET | Pro | Treasury holdings by coin (entity type in path: `companies`, `governments`, etc.) | `order`, `per_page`, `page` |
| `/public_treasury/{entity_id}` | GET | Pro | Treasury holdings by entity | — |
| `/public_treasury/{entity_id}/{coin_id}/holding_chart` | GET | Pro | Historical treasury holdings | `days` |
| `/public_treasury/{entity_id}/transaction_history` | GET | Pro | Treasury tx history | `per_page`, `page` |
| `/news` | GET | Pro | Crypto news & guides | `page` (max 20), `per_page` (max 20), `coin_id`, `language`, `type` (`all`/`news`/`guides`) |
| `/companies/public_treasury/{coin_id}` | GET | Demo | Legacy alias for companies treasury (BTC/ETH/etc.) | — |

Deprecation notes: none of the above are marked deprecated in the 2026-04 OAS, but `/companies/public_treasury/{coin_id}` is now a legacy alias for the more general `/{entity}/public_treasury/{coin_id}` route. Keep both in the client for back-compat.

### 3.2 Onchain (GeckoTerminal) — under `/onchain/` on Pro, under the same path prefix on Demo

Source: <https://raw.githubusercontent.com/coingecko/coingecko-api-oas/refs/heads/main/onchain-pro.json>.

| Path (under `/api/v3/onchain`) | Verb | Tier | Description |
| --- | --- | --- | --- |
| `/simple/networks/{network}/token_price/{addresses}` | GET | Demo | Token prices by contract (up to 30 demo / 100 pro per request) |
| `/search/pools` | GET | Demo | Search pools & tokens by name/symbol/contract |
| `/networks` | GET | Demo | Supported networks ID map |
| `/networks/{network}/dexes` | GET | Demo | Supported DEXes for a network |
| `/networks/{network}/pools/{address}` | GET | Demo | Specific pool data |
| `/networks/{network}/pools/multi/{addresses}` | GET | Demo | Multiple pools (CSV of addresses) |
| `/networks/trending_pools` | GET | Demo | Trending pools across all networks |
| `/networks/{network}/trending_pools` | GET | Demo | Trending pools for one network |
| `/networks/{network}/pools` | GET | Demo | Top pools for a network |
| `/networks/{network}/dexes/{dex}/pools` | GET | Demo | Top pools for a DEX |
| `/networks/new_pools` | GET | Demo | Newest pools across networks |
| `/networks/{network}/new_pools` | GET | Demo | Newest pools for a network |
| `/pools/megafilter` | GET | Analyst+ 🔥 | Multi-criteria pool filter |
| `/pools/trending_search` | GET | Pro 💼 | Trending-search pools |
| `/networks/{network}/tokens/{token_address}/pools` | GET | Demo | Pools for a token |
| `/networks/{network}/tokens/{address}` | GET | Demo | Token data |
| `/networks/{network}/tokens/multi/{addresses}` | GET | Demo | Multiple tokens |
| `/networks/{network}/tokens/{address}/info` | GET | Demo | Token metadata |
| `/networks/{network}/pools/{pool_address}/info` | GET | Demo | Pool metadata (base/quote token info) |
| `/tokens/info_recently_updated` | GET | Demo | 100 recently updated tokens |
| `/networks/{network_id}/tokens/{token_address}/top_traders` | GET | Pro 💼 | Top traders of a token |
| `/networks/{network}/tokens/{address}/top_holders` | GET | Pro 💼 | Top holders of a token |
| `/networks/{network}/tokens/{token_address}/holders_chart` | GET | Pro 💼 | Historical holders chart |
| `/networks/{network}/pools/{pool_address}/ohlcv/{timeframe}` | GET | Demo | Pool OHLCV (`timeframe`: `day`/`hour`/`minute`, with `aggregate` query) |
| `/networks/{network}/tokens/{token_address}/ohlcv/{timeframe}` | GET | Pro 💼 | Token OHLCV |
| `/networks/{network}/pools/{pool_address}/trades` | GET | Demo | Past 24h trades (max 300) |
| `/networks/{network}/tokens/{token_address}/trades` | GET | Pro 💼 | Past 24h trades for token |
| `/categories` | GET | Pro 💼 | Onchain categories list |
| `/categories/{category_id}/pools` | GET | Pro 💼 | Pools in a category |

Legend: 💼 = paid-plan only, 🔥 = "premium" pro-tier endpoint (typically Analyst+).

Common query params for list endpoints: `include` (CSV of related resources like `base_token,quote_token,dex`), `page`, `sort`, `duration`, `include_volume_breakdown`, `include_network`.

---

## 4. Demo / Public API subset

Source: <https://raw.githubusercontent.com/coingecko/coingecko-api-oas/refs/heads/main/coingecko-demo.json>.

Endpoints **present** on Demo (core + limited onchain):

```
/ping
/simple/price
/simple/token_price/{id}
/simple/supported_vs_currencies
/search
/search/trending
/coins/list
/coins/markets
/coins/{id}
/coins/{id}/tickers
/coins/{id}/history
/coins/{id}/market_chart
/coins/{id}/market_chart/range
/coins/{id}/ohlc
/coins/{id}/contract/{contract_address}
/coins/{id}/contract/{contract_address}/market_chart
/coins/{id}/contract/{contract_address}/market_chart/range
/coins/categories/list
/coins/categories
/asset_platforms
/token_lists/{asset_platform_id}/all.json
/exchanges
/exchanges/list
/exchanges/{id}
/exchanges/{id}/tickers
/exchanges/{id}/volume_chart
/derivatives
/derivatives/exchanges
/derivatives/exchanges/{id}
/derivatives/exchanges/list
/entities/list
/{entity}/public_treasury/{coin_id}
/public_treasury/{entity_id}
/public_treasury/{entity_id}/{coin_id}/holding_chart
/public_treasury/{entity_id}/transaction_history
/nfts/list
/nfts/{id}
/nfts/{asset_platform_id}/contract/{contract_address}
/exchange_rates
/global
/global/decentralized_finance_defi
```

Endpoints **NOT present** on Demo (Pro-only; throw `10005` if called with Demo key):
- `/key`
- `/coins/list/new`
- `/coins/top_gainers_losers`
- `/coins/{id}/ohlc/range`
- `/coins/{id}/circulating_supply_chart`, `/coins/{id}/circulating_supply_chart/range`
- `/coins/{id}/total_supply_chart`, `/coins/{id}/total_supply_chart/range`
- `/exchanges/{id}/volume_chart/range`
- `/nfts/list_with_market_data`, `/nfts/{id}/market_chart`, `/nfts/{id}/tickers`, `/nfts/{asset_platform_id}/contract/{contract_address}/market_chart`, `/nfts/markets`
- `/global/market_cap_chart`
- `/news`
- Onchain: `/pools/megafilter`, `/pools/trending_search`, `/categories`, `/categories/{id}/pools`, all "Top Holders / Top Traders / holders_chart" routes, token-level OHLCV & trades.

Differences **within shared endpoints**:
- `/search/trending` on Demo: max 15 coins / 7 NFTs / 6 categories. Analyst+ raises to 30/10/10.
- Historical depth capped at ~1 year on Demo; Pro returns data back to 2013.
- Update frequency: most market-data endpoints tick every 60s on Demo vs 20s-30s on Pro (e.g. `/simple/price` is 20s on Pro, ~60s on Demo).
- Onchain multi-address endpoints: Demo caps at 30 addresses per request, Pro at 100.

---

## 5. WebSocket Beta

Connection: `wss://stream.coingecko.com/v1`  
Auth: pass Pro API key via header `x-cg-pro-api-key: <KEY>` on the upgrade request, or append `?x_cg_pro_api_key=<KEY>` to the URL.  
Plan requirement: **Analyst or higher**.  
Pricing: **0.1 credit per message received** (not per subscription).  
Max concurrent connections: **10** per key.  
Max subscriptions per socket per channel: **100**.

Sources: <https://docs.coingecko.com/websocket/cgsimpleprice>, <https://docs.coingecko.com/reference/wss-onchain-trade>, <https://docs.coingecko.com/websocket/wssonchainohlcv>, <https://docs.coingecko.com/docs/g1-onchainsimpletokenprice>.

### 5.1 Protocol

ActionCable-flavoured JSON RPC (same wire-format as Rails/ActionCable). Every message has:
- `command`: `subscribe` | `unsubscribe` | `message`
- `identifier`: a JSON-encoded string describing the channel (yes — a stringified JSON inside the outer JSON).
- `data`: another JSON-encoded string carrying the inner command payload.

Heartbeat: server sends a `ping` frame every **10s**. Client must reply to WebSocket pong within **20s** or the server disconnects. Most C# WebSocket libraries (`System.Net.WebSockets.ClientWebSocket` with `KeepAliveInterval`) handle this automatically, but the library should validate with a dedicated keep-alive task and reconnect logic with exponential backoff.

Reconnection: the spec recommends exponential backoff; no server-side resume token, so after reconnect the client must re-subscribe + re-set its token/pool list.

Null values: any field may be `null` when upstream data is unavailable.

### 5.2 Channels

| Code | Channel name | Action | Purpose | Update cadence |
| ---- | ------------ | ------ | ------- | -------------- |
| C1   | `CGSimplePrice` | `set_tokens` / `unset_tokens` | CoinGecko.com aggregated prices | ~10s for top coins |
| G1   | `OnchainSimpleTokenPrice` | `set_tokens` / `unset_tokens` | GeckoTerminal token prices | sub-second for hot pools |
| G2   | `OnchainTrade` | `set_pools` / `unset_pools` | Pool swaps (trades) | as fast as 0.1s |
| G3   | `OnchainOHLCV` | `set_pools` / `unset_pools` | Pool OHLCV candles | as fast as 1s |

#### CGSimplePrice (C1)

```json
// subscribe
{"command":"subscribe","identifier":"{\"channel\":\"CGSimplePrice\"}"}
// server replies
{"type":"confirm_subscription","identifier":"{\"channel\":\"CGSimplePrice\"}"}

// set tokens
{"command":"message","identifier":"{\"channel\":\"CGSimplePrice\"}","data":"{\"coin_id\":[\"ethereum\",\"bitcoin\"],\"vs_currencies\":[\"usd\",\"eur\"],\"action\":\"set_tokens\"}"}
// server acks
{"code":2000,"message":"Subscription is successful for ethereum"}

// price update (push)
{"c":"C1","i":"ethereum","vs":"usd","p":2591.08,"pp":1.38,"m":312938652962.8,"v":20460612214.8,"t":1747808150.269}

// unsubscribe one
{"command":"message","identifier":"{\"channel\":\"CGSimplePrice\"}","data":"{\"coin_id\":[\"ethereum\"],\"action\":\"unset_tokens\"}"}
// unsubscribe entire channel
{"command":"unsubscribe","identifier":"{\"channel\":\"CGSimplePrice\"}"}
```

Fields: `c` channel code, `i` coin id, `vs` quote currency, `p` price, `pp` 24h pct change, `m` market cap, `v` 24h volume, `t` unix timestamp (float seconds).

#### OnchainSimpleTokenPrice (G1)

Same envelope, channel = `OnchainSimpleTokenPrice`, identifier key = `"network_id:token_addresses"` with values like `["eth:0xa0b8...","bsc:0x..."]`, action `set_tokens`. Push payload similar to C1 but also includes `n` (network), `ta` (token address), FDV/reserve fields.

#### OnchainTrade (G2)

```json
{"command":"message","identifier":"{\"channel\":\"OnchainTrade\"}","data":"{\"network_id:pool_addresses\":[\"bsc:0x172fcd41e0913e95784454622d1c3724f546f849\"],\"action\":\"set_pools\"}"}
```

Push fields: `ch` (G2), `n` network, `pa` pool address, `tx` tx hash, `ty` trade type (`b` buy / `s` sell), `to` token amount, `toq` quote token amount, `vo` USD volume, `pc` price in native, `pu` price in USD, `t` unix ms timestamp.

#### OnchainOHLCV (G3)

```json
{"command":"message","identifier":"{\"channel\":\"OnchainOHLCV\"}","data":"{\"network_id:pool_addresses\":[\"bsc:0x172fcd...\"],\"interval\":\"1m\",\"token\":\"base\",\"action\":\"set_pools\"}"}
```

Intervals: `1s`, `1m`, `5m`, `15m`, `1h`, `2h`, `4h`, `8h`, `12h`, `1d`.  
`token`: `base` or `quote`.  
Push fields: `ch` (G3), `n`, `pa`, `to`, `i` (interval), `o`, `h`, `l`, `c`, `v`, `t` (candle start unix).  
Subscription quota counts **unique (pool × interval × token) combinations** against the 100/channel/socket cap.

### 5.3 AsyncAPI spec

CoinGecko publishes an AsyncAPI document referenced from the llms.txt (`asyncapi` entry) but the canonical URL was not directly reachable during this research. The OAS repository may gain an `asyncapi.json` alongside the OpenAPI files — library maintainers should watch <https://github.com/coingecko/coingecko-api-oas>.

---

## 6. OpenAPI / Postman / SDKs

### 6.1 OpenAPI 3.0

Official OpenAPI JSON files (MIT licensed): <https://github.com/coingecko/coingecko-api-oas>

| File | Raw URL | Covers |
| ---- | ------- | ------ |
| `coingecko-pro.json` | `https://raw.githubusercontent.com/coingecko/coingecko-api-oas/refs/heads/main/coingecko-pro.json` | Pro REST catalogue |
| `coingecko-demo.json` | `.../coingecko-demo.json` | Demo REST catalogue |
| `onchain-pro.json` | `.../onchain-pro.json` | GeckoTerminal (Pro) |
| `onchain-demo.json` | `.../onchain-demo.json` | GeckoTerminal (Demo) |

Legacy Swagger JSON (v3, still live): <https://www.coingecko.com/api/documentations/v3/swagger.json>

**Recommendation for C# library**: use the OAS repo as the source of truth. Build `CoinGecko.Client.CodeGen` that generates request/response DTOs + interface stubs from the JSON files via NSwag or Kiota. Keep the public hand-written client wrapping the generated primitives so downstream users get ergonomic C# naming while we stay in lockstep with upstream schema changes.

### 6.2 Postman / other

No official Postman collection is published. Third-party collections exist on postman.com but are unofficial.

### 6.3 Official & community SDKs

- **Official TypeScript SDK**: `@coingecko/coingecko-typescript` (npm) — generated, kept current with OAS.
- **Official Python SDK**: `pycoingecko` (official) — REST wrapper.
- **Official MCP server**: `@coingecko/coingecko-mcp` (npm) — remote + local.
- **Official CLI**: CoinGecko CLI (Go binary).
- **Official Google Sheets add-on**: listed in AI Hub.
- **Unofficial**: `Drakkar-Software/coingecko-openapi-clients` (TypeScript/Python codegen), `crazyrabbitLTC/mcp-coingecko-server` (Anthropic-compatible MCP wrapper), many community C# wrappers (none currently maintained against Pro + Onchain + WebSocket combined — this is the market gap).

---

## 7. API Quirks & Conventions — design-impacting notes

These are the details that surprise implementers and should be codified in the C# library.

1. **`vs_currency` vs `vs_currencies`** — singular for market-level endpoints (`/coins/markets`, `/coins/{id}/market_chart`), **plural CSV** for simple-price endpoints (`/simple/price`, `/simple/token_price`). The client MUST NOT blindly unify these.
2. **`ids` / `names` / `symbols` are CSV strings**, not arrays. The client should accept `IEnumerable<string>` in C# and serialize to CSV with `string.Join(",", ...)`.
3. **Coin ID vs slug vs symbol**: CoinGecko `id` (e.g. `bitcoin`) is the canonical key. Symbol is ambiguous (many coins share `BTC`/`ETH`/etc.). Symbol lookup requires `include_tokens=top|all` as a disambiguator. Provide a helper `ResolveIdAsync(symbol)` that hits `/coins/list` and caches aggressively.
4. **Asset platform ID vs network ID** — they are **not the same**. CoinGecko's `asset_platforms` use names like `ethereum`, `binance-smart-chain`; GeckoTerminal's `networks` use short codes like `eth`, `bsc`, `polygon_pos`. The client should expose both enums and a cross-mapping table.
5. **Contract address casing**: Ethereum-family addresses are case-insensitive for CoinGecko matching, but the library should always lowercase them before caching keys. Solana addresses are case-sensitive.
6. **Timestamps**:
   - REST chart endpoints return `[unix_ms, value]` tuples (milliseconds).
   - REST historical endpoints (`/coins/{id}/history`) accept `dd-mm-yyyy` strings.
   - Range endpoints (`/coins/{id}/market_chart/range`) accept `from`/`to` as **unix seconds**.
   - WebSocket C1 returns seconds as `float`; G2 returns **milliseconds** as int; G3 returns candle-start **seconds**.
   - The library should expose `DateTimeOffset` on the public surface and handle each encoding internally.
7. **Pagination**: everywhere uses `page` + `per_page` (max 250). No cursor pagination. `Link` response headers are not returned. Clients must inspect array length to detect end of results.
8. **Response enveloping**: core endpoints return bare arrays/objects. Onchain endpoints use JSON:API-style `{ data: {...}, included: [...], meta: {...}, links: {...} }`. The C# library needs **two deserialization strategies** — plain for core and JSON:API-lite for `/onchain/*`.
9. **Null-heavy fields**: `max_supply`, `roi`, `fully_diluted_valuation`, `ath_date`, numerous `community_data` fields can all be null; all numeric DTO fields should be `decimal?`, date fields `DateTimeOffset?`.
10. **Price precision**: `precision` query param accepts `full` or `0`-`18`. Without it, floats can lose precision on sub-cent tokens. The library should default to `precision=full` for any token-level endpoint to avoid silent truncation in `double` → C# clients should use `decimal`.
11. **Rate-limit counting**: every HTTP response counts against minute rate limits regardless of 2xx/4xx/5xx. Client-side throttle must increment BEFORE sending.
12. **4xx doesn't burn credits, but 429 does count against rate** — retry math must track both dimensions.
13. **429 + Retry-After** — CoinGecko sometimes returns `Retry-After` headers on 429 but not reliably. Library should assume up to 60s backoff.
14. **Caching**: CoinGecko cache TTLs vary by endpoint (20s, 60s, 5min, 10min). Library can expose a `CacheHint` from response metadata for callers who want client-side caching.
15. **Cloudflare 1020**: if a request returns 1020 with an HTML body, the user's UA or IP is blocked — bubble up a distinct exception with guidance to set a proper `User-Agent`.
16. **CORS**: only an issue for browser callers; irrelevant for a server-side C# SDK.
17. **Treasury route quirk**: `/{entity}/public_treasury/{coin_id}` uses the **entity type** (e.g. `companies`, `governments`) as a path segment — the library must not confuse it with entity ID routes (`/public_treasury/{entity_id}`).
18. **Token lists end in `.json`**: `/token_lists/{asset_platform_id}/all.json` literally has `.json` in the path — don't strip it.
19. **`dex_pair_format` parameter**: appears on `/coins/{id}`, `/coins/{id}/tickers`, and exchange ticker endpoints. Values: `contract_address` or `symbol`. Changes the shape of ticker identifiers.
20. **Top gainers/losers** accepts `duration` values `1h`, `24h`, `7d`, `14d`, `30d`, `60d`, `1y`.
21. **Chart `interval` auto-adjustment**: omit `interval` for automatic granularity (5m for ≤1d, hourly for ≤90d, daily beyond). Explicit values: `5m`, `hourly`, `daily`. OHLC endpoint uses fixed windows based on `days` (1=30min candles, 7/14/30/90=4h, 180/365=4d).

---

## 8. Recommended C# library architecture (summary)

- `CoinGecko.Client` — core REST (httpclientfactory, Polly-based retry, 429 handling, typed exceptions).
- `CoinGecko.Client.Core` — namespaces: `Coins`, `Simple`, `Markets`, `Exchanges`, `Derivatives`, `Nfts`, `Categories`, `Global`, `Treasury`, `News`, `Search`.
- `CoinGecko.Client.Onchain` — GeckoTerminal endpoints, JSON:API deserializer, network ID enum, DEX ID constants.
- `CoinGecko.Client.WebSocket` — ActionCable-flavoured client, channel subscriptions with `IObservable<PriceUpdate>` / `IAsyncEnumerable<>` surfaces, heartbeat + exponential backoff reconnect.
- `CoinGecko.Client.Mcp` (optional) — MCP client wrapper for agent scenarios (Option B in §2.1).
- `CoinGecko.Client.X402` (optional) — wallet-signing extension for pay-per-use endpoints.
- `CoinGecko.Client.CodeGen` — internal project generating request/response DTOs from the four OAS JSON files; regenerated in CI.

Headers the base HTTP handler must always set: `x-cg-{demo|pro}-api-key`, `Accept: application/json`, `Accept-Encoding: gzip, deflate, br`, `User-Agent: CoinGeckoSharp/<version> (+https://github.com/<owner>/<repo>)`.

Public API surface should be async-only (`Task<T>`), accept `CancellationToken`, use `decimal` for monetary values, and expose strongly-typed enums for every string-enum parameter (order, interval, duration, network, precision, include flags, entity type, language, news type, etc.).
