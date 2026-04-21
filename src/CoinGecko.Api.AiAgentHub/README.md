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
