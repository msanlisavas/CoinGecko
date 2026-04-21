using System.Reflection;
using CoinGecko.Api.AiAgentHub.Mcp;

namespace CoinGecko.Api.AiAgentHub.Mcp.Tests;

public class PublicApiShapeTests
{
    [Fact]
    public void CoinGeckoMcp_exposes_ConnectAsync_and_CreateClientAsync()
    {
        var t = typeof(CoinGeckoMcp);
        t.GetMethod("ConnectAsync", BindingFlags.Public | BindingFlags.Static).ShouldNotBeNull();
        t.GetMethod("CreateClientAsync", BindingFlags.Public | BindingFlags.Static).ShouldNotBeNull();
    }

    [Fact]
    public void CoinGeckoMcpOptions_exposes_BaseAddress_Transport_CallTimeout()
    {
        var t = typeof(CoinGeckoMcpOptions);
        t.GetProperty("BaseAddress").ShouldNotBeNull();
        t.GetProperty("Transport").ShouldNotBeNull();
        t.GetProperty("CallTimeout").ShouldNotBeNull();
    }
}
