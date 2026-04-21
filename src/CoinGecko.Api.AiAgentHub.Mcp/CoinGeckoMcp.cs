using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace CoinGecko.Api.AiAgentHub.Mcp;

/// <summary>Entry points for connecting to CoinGecko's hosted MCP server.</summary>
public static class CoinGeckoMcp
{
    private const string DemoHost = "https://mcp.api.coingecko.com";
    private const string ProHost = "https://mcp.pro-api.coingecko.com";

    /// <summary>
    /// Create a low-level <see cref="McpClient"/> connected to CoinGecko's hosted MCP server.
    /// Use when you want fine-grained control over the MCP session (e.g., progress notifications,
    /// custom handlers). For the common case of "give me tools," use <see cref="ConnectAsync"/> instead.
    /// </summary>
    /// <param name="apiKey">CoinGecko API key — sent as <c>Authorization: Bearer {apiKey}</c>.</param>
    /// <param name="plan">Plan tier (picks between the demo and pro MCP hosts).</param>
    /// <param name="options">Transport + timeout overrides.</param>
    /// <param name="ct">Cancellation.</param>
    public static async Task<McpClient> CreateClientAsync(
        string apiKey,
        CoinGeckoPlan plan = CoinGeckoPlan.Demo,
        CoinGeckoMcpOptions? options = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        var opts = options ?? new CoinGeckoMcpOptions();

        var baseUri = ResolveBaseUri(opts.BaseAddress, plan, opts.Transport);

        var transportOptions = new HttpClientTransportOptions
        {
            Endpoint = baseUri,
            Name = "CoinGecko MCP",
            TransportMode = opts.Transport switch
            {
                McpTransport.Sse => HttpTransportMode.Sse,
                _ => HttpTransportMode.StreamableHttp,
            },
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {apiKey}",
            },
        };

        var transport = new HttpClientTransport(transportOptions);
        return await McpClient.CreateAsync(transport, cancellationToken: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Connect to CoinGecko's hosted MCP server, list its tools, and wrap them as
    /// <see cref="AIFunction"/>s for use with any <c>Microsoft.Extensions.AI</c> <c>IChatClient</c>.
    /// </summary>
    /// <param name="apiKey">API key.</param>
    /// <param name="plan">Plan tier.</param>
    /// <param name="options">Transport + timeout overrides.</param>
    /// <param name="ct">Cancellation.</param>
    public static async Task<IReadOnlyList<AIFunction>> ConnectAsync(
        string apiKey,
        CoinGeckoPlan plan = CoinGeckoPlan.Demo,
        CoinGeckoMcpOptions? options = null,
        CancellationToken ct = default)
    {
        var client = await CreateClientAsync(apiKey, plan, options, ct).ConfigureAwait(false);
        // McpClientTool extends AIFunction, so the cast is direct.
        var tools = await client.ListToolsAsync(cancellationToken: ct).ConfigureAwait(false);
        return tools.Cast<AIFunction>().ToArray();
    }

    internal static Uri ResolveBaseUri(Uri? userBase, CoinGeckoPlan plan, McpTransport transport)
    {
        if (userBase is not null)
        {
            return userBase;
        }

        var host = plan == CoinGeckoPlan.Demo ? DemoHost : ProHost;
        var path = transport == McpTransport.Sse ? "/sse" : "/mcp";
        return new Uri(host + path);
    }
}
