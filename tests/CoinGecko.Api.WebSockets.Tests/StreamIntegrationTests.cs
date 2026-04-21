using CoinGecko.Api.WebSockets;
using CoinGecko.Api.WebSockets.Ticks;
using CoinGecko.Api.WebSockets.Tests.Infra;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoinGecko.Api.WebSockets.Tests;

public sealed class StreamIntegrationTests
{
    private static CoinGeckoStream Build(Uri serverUri, Action<CoinGeckoStreamOptions>? tweak = null)
    {
        var opts = new CoinGeckoStreamOptions
        {
            BaseAddress = serverUri,
            ApiKey = "test-key",
            AutoReconnect = false,
            HeartbeatTimeout = TimeSpan.FromSeconds(30), // default; override per test
            MaxReconnectAttempts = 3,
        };
        tweak?.Invoke(opts);
        return new CoinGeckoStream(opts, NullLogger<CoinGeckoStream>.Instance);
    }

    [Fact]
    public async Task ConnectAsync_transitions_through_Connecting_to_Connected()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri);

        var states = new List<StreamState>();
        stream.StateChanged += (_, e) => states.Add(e.Current);

        await stream.ConnectAsync(TestContext.Current.CancellationToken);

        stream.State.ShouldBe(StreamState.Connected);
        states.ShouldContain(StreamState.Connecting);
        states.ShouldContain(StreamState.Connected);
    }

    [Fact]
    public async Task DisconnectAsync_transitions_to_Disconnected()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri);

        await stream.ConnectAsync(TestContext.Current.CancellationToken);
        await stream.DisconnectAsync(TestContext.Current.CancellationToken);

        stream.State.ShouldBe(StreamState.Disconnected);
    }

    [Fact]
    public async Task SubscribeCoinPricesAsync_receives_canned_C1_push()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri);
        await stream.ConnectAsync(TestContext.Current.CancellationToken);

        CoinPriceTick? received = null;
        var signal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var sub = await stream.SubscribeCoinPricesAsync(
            coinIds: ["ethereum"],
            vsCurrencies: ["usd"],
            onTick: tick => { received = tick; signal.TrySetResult(true); },
            TestContext.Current.CancellationToken);

        // Give the client a moment to send the subscribe + set_tokens frames before we push.
        await Task.Delay(100, TestContext.Current.CancellationToken);

        await server.PushAsync("""
            {"identifier":"{\"channel\":\"CGSimplePrice\"}","message":{"c":"C1","i":"ethereum","vs":"usd","p":2591.08,"pp":1.38,"m":312938652962.8,"v":20460612214.8,"t":1747808150.269}}
            """, TestContext.Current.CancellationToken);

        await signal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        received.ShouldNotBeNull();
        received!.CoinId.ShouldBe("ethereum");
        received.Price.ShouldBe(2591.08m);

        await sub.DisposeAsync();
    }

    [Fact]
    public async Task SubscribeDexTradesAsync_receives_canned_G2_push()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri);
        await stream.ConnectAsync(TestContext.Current.CancellationToken);

        DexTrade? received = null;
        var signal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var sub = await stream.SubscribeDexTradesAsync(
            networkAndPoolAddresses: ["bsc:0x172fcd41e0913e95784454622d1c3724f546f849"],
            onTrade: t => { received = t; signal.TrySetResult(true); },
            TestContext.Current.CancellationToken);

        await Task.Delay(100, TestContext.Current.CancellationToken);

        await server.PushAsync("""
            {"identifier":"{\"channel\":\"OnchainTrade\"}","message":{"ch":"G2","n":"bsc","pa":"0x172fcd41e0913e95784454622d1c3724f546f849","tx":"0xabc","ty":"b","to":1.5,"toq":3000,"vo":2500,"pc":0.01,"pu":1.67,"t":1747808150000}}
            """, TestContext.Current.CancellationToken);

        await signal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        received.ShouldNotBeNull();
        received!.NetworkId.ShouldBe("bsc");
        received.TxHash.ShouldBe("0xabc");
        received.Timestamp.ToUnixTimeMilliseconds().ShouldBe(1747808150000L);

        await sub.DisposeAsync();
    }

    [Fact]
    public async Task SubscribeDexOhlcvAsync_receives_canned_G3_push()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri);
        await stream.ConnectAsync(TestContext.Current.CancellationToken);

        OnchainOhlcvCandle? received = null;
        var signal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var sub = await stream.SubscribeDexOhlcvAsync(
            networkAndPoolAddresses: ["bsc:0x172fcd41"],
            interval: "1m",
            token: "base",
            onCandle: c => { received = c; signal.TrySetResult(true); },
            TestContext.Current.CancellationToken);

        await Task.Delay(100, TestContext.Current.CancellationToken);

        await server.PushAsync("""
            {"identifier":"{\"channel\":\"OnchainOHLCV\"}","message":{"ch":"G3","n":"bsc","pa":"0x172fcd41","to":"base","i":"1m","o":1.0,"h":2.0,"l":0.5,"c":1.5,"v":1000,"t":1747808100}}
            """, TestContext.Current.CancellationToken);

        await signal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        received.ShouldNotBeNull();
        received!.Interval.ShouldBe("1m");
        received.Open.ShouldBe(1.0m);
        received.Close.ShouldBe(1.5m);

        await sub.DisposeAsync();
    }

    [Fact]
    public async Task Subscription_dispose_records_unset_frame_on_server()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri);
        await stream.ConnectAsync(TestContext.Current.CancellationToken);

        var sub = await stream.SubscribeCoinPricesAsync(
            ["bitcoin"], ["usd"], _ => { },
            TestContext.Current.CancellationToken);

        // Wait for subscribe + set_tokens frames to arrive at server.
        await Task.Delay(150, TestContext.Current.CancellationToken);
        await sub.DisposeAsync();
        await Task.Delay(150, TestContext.Current.CancellationToken);

        var frames = server.ReceivedFrames;
        // The data field is a JSON-encoded string, so the action key appears as \":\"unset_tokens\"
        // in the raw wire frame recorded by the server.
        frames.ShouldContain(f => f.Contains("unset_tokens", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Subscription_cap_throws_on_101st_subscription()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri, o => o.MaxSubscriptionsPerChannel = 2);
        await stream.ConnectAsync(TestContext.Current.CancellationToken);

        await stream.SubscribeCoinPricesAsync(["a"], ["usd"], _ => { }, TestContext.Current.CancellationToken);
        await stream.SubscribeCoinPricesAsync(["b"], ["usd"], _ => { }, TestContext.Current.CancellationToken);

        await Should.ThrowAsync<CoinGeckoStreamException>(
            () => stream.SubscribeCoinPricesAsync(["c"], ["usd"], _ => { }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Heartbeat_timeout_triggers_reconnect_when_AutoReconnect_is_true()
    {
        await using var server = await FakeCoinGeckoStreamServer.StartAsync();
        await using var stream = Build(server.Uri, o =>
        {
            o.AutoReconnect = true;
            o.MaxReconnectAttempts = 2;
            o.HeartbeatTimeout = TimeSpan.FromMilliseconds(500);
        });

        var transitioned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        stream.StateChanged += (_, e) =>
        {
            if (e.Current == StreamState.Reconnecting) { transitioned.TrySetResult(true); }
        };

        await stream.ConnectAsync(TestContext.Current.CancellationToken);

        // Wait past heartbeat timeout with no server activity.
        await transitioned.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        stream.State.ShouldBeOneOf(StreamState.Reconnecting, StreamState.Connected, StreamState.Faulted);
    }

    [Fact]
    public async Task MaxReconnectAttempts_exhausted_transitions_to_Faulted()
    {
        var server = await FakeCoinGeckoStreamServer.StartAsync();
        var faulted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var stream = Build(server.Uri, o =>
        {
            o.AutoReconnect = true;
            o.MaxReconnectAttempts = 1;
            o.HeartbeatTimeout = TimeSpan.FromMilliseconds(300);
        });

        stream.StateChanged += (_, e) =>
        {
            if (e.Current == StreamState.Faulted) { faulted.TrySetResult(true); }
        };

        try
        {
            await stream.ConnectAsync(TestContext.Current.CancellationToken);

            // Kill the server so reconnect will fail.
            await server.DisposeAsync();

            await faulted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            stream.State.ShouldBe(StreamState.Faulted);
            stream.LastException.ShouldNotBeNull();
        }
        finally
        {
            // server already disposed above; just dispose stream (handled by await using)
        }
    }
}
