using CoinGecko.Api.WebSockets;

namespace CoinGecko.Api.WebSockets.Tests;

public class OptionsAndStateTests
{
    [Fact]
    public void StreamState_values_cover_lifecycle_transitions()
    {
        var all = Enum.GetValues<StreamState>();
        all.ShouldContain(StreamState.Disconnected);
        all.ShouldContain(StreamState.Connecting);
        all.ShouldContain(StreamState.Connected);
        all.ShouldContain(StreamState.Reconnecting);
        all.ShouldContain(StreamState.Faulted);
        ((int)default(StreamState)).ShouldBe(0);
    }

    [Fact]
    public void Options_defaults()
    {
        var o = new CoinGeckoStreamOptions();
        o.ApiKey.ShouldBeNull();
        o.BaseAddress.ShouldBe(new Uri("wss://stream.coingecko.com/v1"));
        o.AutoReconnect.ShouldBeTrue();
        o.MaxReconnectAttempts.ShouldBe(10);
        o.HeartbeatTimeout.ShouldBe(TimeSpan.FromSeconds(25)); // server pings every ~10s; 25s covers two missed pings
        o.MaxSubscriptionsPerChannel.ShouldBe(100);
        o.ReceiveBufferSize.ShouldBe(16 * 1024);
    }
}
