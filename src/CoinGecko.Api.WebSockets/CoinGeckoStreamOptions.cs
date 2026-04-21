namespace CoinGecko.Api.WebSockets;

/// <summary>Configuration for a CoinGecko WebSocket stream client.</summary>
public sealed class CoinGeckoStreamOptions
{
    /// <summary>Pro-tier API key (Analyst+ plan required by CoinGecko).</summary>
    public string? ApiKey { get; set; }

    /// <summary>WebSocket base URL. Override for tests or proxies.</summary>
    public Uri BaseAddress { get; set; } = new("wss://stream.coingecko.com/v1");

    /// <summary>Whether to reconnect automatically after a disconnect.</summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>Maximum reconnect attempts before transitioning to <see cref="StreamState.Faulted"/>.</summary>
    public int MaxReconnectAttempts { get; set; } = 10;

    /// <summary>If no server message / ping arrives within this window, the connection is considered dead.</summary>
    public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(25);

    /// <summary>Client-side cap on subscriptions per channel (mirrors the upstream 100 per socket).</summary>
    public int MaxSubscriptionsPerChannel { get; set; } = 100;

    /// <summary>Receive buffer size for individual WebSocket frames.</summary>
    public int ReceiveBufferSize { get; set; } = 16 * 1024;

    /// <summary>Base WS KeepAliveInterval (the server pings every ~10s).</summary>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
}
