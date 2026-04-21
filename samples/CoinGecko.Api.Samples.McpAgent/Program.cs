using CoinGecko.Api.AiAgentHub.Mcp;

var apiKey = Environment.GetEnvironmentVariable("COINGECKO_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.Error.WriteLine("Set COINGECKO_API_KEY first.");
    return 1;
}

try
{
    var tools = await CoinGeckoMcp.ConnectAsync(apiKey, CoinGeckoPlan.Demo);
    Console.WriteLine($"Connected. Fetched {tools.Count} MCP tools from CoinGecko:");
    foreach (var t in tools)
    {
        Console.WriteLine($"  - {t.Name}");
        Console.WriteLine($"    {t.Description}");
        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"MCP connect failed: {ex.Message}");
    return 2;
}

return 0;
