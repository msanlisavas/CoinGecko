namespace CoinGecko.Api.WebSockets;

/// <summary>Thrown by the CoinGecko WebSocket stream client for protocol / subscription errors.</summary>
public sealed class CoinGeckoStreamException : Exception
{
    /// <summary>Create an exception with a message.</summary>
    public CoinGeckoStreamException(string message) : base(message) { }
    /// <summary>Create an exception with a message and inner cause.</summary>
    public CoinGeckoStreamException(string message, Exception inner) : base(message, inner) { }
}
