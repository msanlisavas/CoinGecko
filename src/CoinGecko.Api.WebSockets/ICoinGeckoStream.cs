using CoinGecko.Api.WebSockets.Ticks;

namespace CoinGecko.Api.WebSockets;

/// <summary>Streaming client for CoinGecko's WebSocket API (beta).</summary>
public interface ICoinGeckoStream : IAsyncDisposable
{
    /// <summary>Current state.</summary>
    StreamState State { get; }

    /// <summary>Last exception that caused a transition away from <see cref="StreamState.Connected"/>, or null if healthy.</summary>
    Exception? LastException { get; }

    /// <summary>Raised on every state transition.</summary>
    event EventHandler<StreamStateChangedEventArgs>? StateChanged;

    /// <summary>Open the WebSocket and enter <see cref="StreamState.Connected"/>.</summary>
    Task ConnectAsync(CancellationToken ct = default);

    /// <summary>Gracefully close. Enters <see cref="StreamState.Disconnected"/>.</summary>
    Task DisconnectAsync(CancellationToken ct = default);

    /// <summary>C1: subscribe to coin prices. Disposing the returned handle unsubscribes just this subscription.</summary>
    Task<IAsyncDisposable> SubscribeCoinPricesAsync(
        IReadOnlyList<string> coinIds,
        IReadOnlyList<string> vsCurrencies,
        Action<CoinPriceTick> onTick,
        CancellationToken ct = default);

    /// <summary>G1: subscribe to onchain token prices. Tokens are <c>network_id:address</c> pairs.</summary>
    Task<IAsyncDisposable> SubscribeOnchainTokenPricesAsync(
        IReadOnlyList<string> networkAndTokenAddresses,
        Action<OnchainTokenPriceTick> onTick,
        CancellationToken ct = default);

    /// <summary>G2: subscribe to DEX pool trades. Pools are <c>network_id:pool_address</c> pairs.</summary>
    Task<IAsyncDisposable> SubscribeDexTradesAsync(
        IReadOnlyList<string> networkAndPoolAddresses,
        Action<DexTrade> onTrade,
        CancellationToken ct = default);

    /// <summary>G3: subscribe to DEX OHLCV candles.</summary>
    Task<IAsyncDisposable> SubscribeDexOhlcvAsync(
        IReadOnlyList<string> networkAndPoolAddresses,
        string interval,
        string token,
        Action<OnchainOhlcvCandle> onCandle,
        CancellationToken ct = default);
}
