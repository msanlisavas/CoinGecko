# CoinGecko.Api.AiAgentHub.Mcp

MCP (Model Context Protocol) client for CoinGecko's hosted MCP servers. Returns `IReadOnlyList<AIFunction>` you can plug into any `Microsoft.Extensions.AI` `IChatClient`.

## Install

```powershell
dotnet add package CoinGecko.Api.AiAgentHub.Mcp
```

## Quickstart

```csharp
using CoinGecko.Api.AiAgentHub.Mcp;
using Microsoft.Extensions.AI;

var tools = await CoinGeckoMcp.ConnectAsync(
    apiKey: Environment.GetEnvironmentVariable("COINGECKO_API_KEY")!,
    plan: CoinGeckoPlan.Pro);

IChatClient chat = /* OpenAI / Anthropic / Azure / Ollama / Gemini — any IChatClient */;
var response = await chat.GetResponseAsync(
    "What's BTC at right now?",
    new ChatOptions { Tools = tools.Cast<AITool>().ToArray() });
```

## Relationship to `CoinGecko.Api.AiAgentHub`

Both packages return `IReadOnlyList<AIFunction>`.

- **`CoinGecko.Api.AiAgentHub`** — tools are thin wrappers over the REST client you control. Zero MCP dependency; deterministic shape.
- **`CoinGecko.Api.AiAgentHub.Mcp`** — tools are fetched from CoinGecko's hosted MCP server. Definitions evolve upstream without a client-side package bump.

Swap between them by changing one line: `CoinGeckoAiTools.Create(gecko)` ↔ `await CoinGeckoMcp.ConnectAsync(apiKey, plan)`.

## Preview status

MCP is a young protocol and the CoinGecko MCP server + the `ModelContextProtocol` .NET SDK are both pre-1.0. This package ships in 0.x-preview and may break with SDK updates.

## License

MIT © msanlisavas
