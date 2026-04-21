namespace CoinGecko.Api.WebSockets;

/// <summary>Lifecycle state of an <see cref="ICoinGeckoStream"/>.</summary>
public enum StreamState
{
    /// <summary>Not connected. Initial state and terminal state after <c>DisconnectAsync</c>.</summary>
    Disconnected = 0,
    /// <summary>Opening the WebSocket handshake.</summary>
    Connecting = 1,
    /// <summary>Open and receiving frames.</summary>
    Connected = 2,
    /// <summary>Temporarily disconnected; attempting to reconnect.</summary>
    Reconnecting = 3,
    /// <summary>Terminal failure. Inspect <c>.Exception</c> for details.</summary>
    Faulted = 4,
}
