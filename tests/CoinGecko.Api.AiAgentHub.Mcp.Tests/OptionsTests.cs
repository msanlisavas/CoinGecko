using CoinGecko.Api.AiAgentHub.Mcp;

namespace CoinGecko.Api.AiAgentHub.Mcp.Tests;

public class OptionsTests
{
    [Fact]
    public void Plan_is_ordered_ascending()
    {
        ((int)CoinGeckoPlan.Demo).ShouldBe(0);
        ((int)CoinGeckoPlan.Basic).ShouldBeGreaterThan((int)CoinGeckoPlan.Demo);
        ((int)CoinGeckoPlan.Enterprise).ShouldBeGreaterThan((int)CoinGeckoPlan.Pro);
    }

    [Fact]
    public void Transport_defaults_to_streamable_http()
    {
        default(McpTransport).ShouldBe(McpTransport.StreamableHttp);
        Enum.GetValues<McpTransport>().ShouldContain(McpTransport.Sse);
    }

    [Fact]
    public void Options_defaults()
    {
        var o = new CoinGeckoMcpOptions();
        o.BaseAddress.ShouldBeNull();
        o.Transport.ShouldBe(McpTransport.StreamableHttp);
        o.CallTimeout.ShouldBe(TimeSpan.FromSeconds(60));
    }
}
