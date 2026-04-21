namespace CoinGecko.Api.AiAgentHub.Mcp;

/// <summary>MCP transport options exposed by the hosted CoinGecko MCP server.</summary>
public enum McpTransport
{
    /// <summary>Streamable HTTP (default, recommended for most callers).</summary>
    StreamableHttp = 0,

    /// <summary>Server-sent events. Use where Streamable HTTP is blocked by an intermediary.</summary>
    Sse = 1,
}
