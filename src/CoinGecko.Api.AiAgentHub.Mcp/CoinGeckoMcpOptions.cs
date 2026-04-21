namespace CoinGecko.Api.AiAgentHub.Mcp;

/// <summary>Configuration for the CoinGecko MCP client.</summary>
public sealed class CoinGeckoMcpOptions
{
    /// <summary>Override the MCP endpoint. Leave null to use the plan-default (<c>mcp.api.coingecko.com/mcp</c> or <c>mcp.pro-api.coingecko.com/mcp</c>).</summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>Transport mechanism.</summary>
    public McpTransport Transport { get; set; } = McpTransport.StreamableHttp;

    /// <summary>Per-call timeout. Applied to tool invocations.</summary>
    public TimeSpan CallTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
