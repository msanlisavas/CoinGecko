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
