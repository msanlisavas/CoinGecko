using CoinGecko.Api.AiAgentHub.Mcp;

namespace CoinGecko.Api.AiAgentHub.Mcp.Tests;

public class ResolveBaseUriTests
{
    [Fact]
    public void Demo_plan_streamable_routes_to_mcp_api()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(null, CoinGeckoPlan.Demo, McpTransport.StreamableHttp);
        uri.ToString().ShouldBe("https://mcp.api.coingecko.com/mcp");
    }

    [Fact]
    public void Paid_plan_streamable_routes_to_mcp_pro_api()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(null, CoinGeckoPlan.Pro, McpTransport.StreamableHttp);
        uri.ToString().ShouldBe("https://mcp.pro-api.coingecko.com/mcp");
    }

    [Fact]
    public void Sse_transport_uses_sse_path()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(null, CoinGeckoPlan.Demo, McpTransport.Sse);
        uri.ToString().ShouldBe("https://mcp.api.coingecko.com/sse");
    }

    [Fact]
    public void User_override_wins()
    {
        var uri = CoinGeckoMcp.ResolveBaseUri(new Uri("https://proxy.example/mcp"), CoinGeckoPlan.Enterprise, McpTransport.Sse);
        uri.ToString().ShouldBe("https://proxy.example/mcp");
    }
}
