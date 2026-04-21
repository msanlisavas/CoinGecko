namespace CoinGecko.Api.WebSockets;

/// <summary>Arguments for <see cref="ICoinGeckoStream.StateChanged"/>.</summary>
public sealed class StreamStateChangedEventArgs(StreamState previous, StreamState current, Exception? error) : EventArgs
{
    /// <summary>Previous state.</summary>
    public StreamState Previous { get; } = previous;
    /// <summary>Current (new) state.</summary>
    public StreamState Current { get; } = current;
    /// <summary>Error that triggered the transition, if any.</summary>
    public Exception? Error { get; } = error;
}
