using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CoinGecko.Api.WebSockets.Internal;
using CoinGecko.Api.WebSockets.Protocol;
using CoinGecko.Api.WebSockets.Ticks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoinGecko.Api.WebSockets;

/// <summary>Concrete streaming client for CoinGecko's WebSocket API (beta).</summary>
public sealed partial class CoinGeckoStream : ICoinGeckoStream
{
    // ── Channel names ────────────────────────────────────────────────────────
    private const string C1Channel = "CGSimplePrice";
    private const string G1Channel = "OnchainSimpleTokenPrice";
    private const string G2Channel = "OnchainTrade";
    private const string G3Channel = "OnchainOHLCV";

    // ── Fields ───────────────────────────────────────────────────────────────
    private readonly CoinGeckoStreamOptions _opts;
    private readonly ILogger _logger;
    private readonly ChannelDispatcher<CoinPriceTick> _c1 = new();
    private readonly ChannelDispatcher<OnchainTokenPriceTick> _g1 = new();
    private readonly ChannelDispatcher<DexTrade> _g2 = new();
    private readonly ChannelDispatcher<OnchainOhlcvCandle> _g3 = new();
    private readonly Dictionary<string, List<RestorableSubscription>> _activeSubs = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _receiveCts;
    private Task? _receiveLoop;
    private DateTimeOffset _lastMessageAt;
    private Timer? _heartbeatTimer;
    private int _reconnectAttempts;
    private bool _disposed;

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Current lifecycle state.</summary>
    public StreamState State { get; private set; } = StreamState.Disconnected;

    /// <summary>Last exception that caused a transition away from <see cref="StreamState.Connected"/>, or <c>null</c> if healthy.</summary>
    public Exception? LastException { get; private set; }

    /// <summary>Number of consecutive reconnect attempts since the last successful connection, or zero when healthy.</summary>
    public int ReconnectAttempts => _reconnectAttempts;

    /// <summary>Raised on every state transition.</summary>
    public event EventHandler<StreamStateChangedEventArgs>? StateChanged;

    /// <summary>Initialise a new stream client.</summary>
    /// <param name="options">Connection options.</param>
    /// <param name="logger">Optional logger; falls back to <see cref="NullLogger{T}"/> when <c>null</c>.</param>
    public CoinGeckoStream(CoinGeckoStreamOptions options, ILogger<CoinGeckoStream>? logger = null)
    {
        _opts = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<CoinGeckoStream>.Instance;
    }

    // ── Connect / Disconnect ─────────────────────────────────────────────────

    /// <summary>Open the WebSocket and enter <see cref="StreamState.Connected"/>.</summary>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _stateLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (State != StreamState.Disconnected && State != StreamState.Faulted)
            {
                throw new CoinGeckoStreamException(
                    $"ConnectAsync called in invalid state {State}. Only Disconnected or Faulted states allow connection.");
            }
        }
        finally
        {
            _stateLock.Release();
        }

        await TransitionToAsync(StreamState.Connecting).ConfigureAwait(false);
        await OpenAndStartReceiveLoopAsync(ct).ConfigureAwait(false);
        await TransitionToAsync(StreamState.Connected).ConfigureAwait(false);
    }

    /// <summary>Gracefully close the WebSocket. Transitions to <see cref="StreamState.Disconnected"/>.</summary>
    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        StopHeartbeatTimer();

        if (_receiveCts is not null)
        {
            await _receiveCts.CancelAsync().ConfigureAwait(false);
        }

        if (_receiveLoop is not null)
        {
            try
            {
                await _receiveLoop.WaitAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // best-effort
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        if (_ws is { State: WebSocketState.Open })
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.WsCloseException(_logger, ex);
            }
        }

        DisposeSocketSafe();
        await TransitionToAsync(StreamState.Disconnected).ConfigureAwait(false);
    }

    // ── Subscribe methods ────────────────────────────────────────────────────

    /// <summary>C1: subscribe to coin prices. Disposing the returned handle unsubscribes just this subscription.</summary>
    public async Task<IAsyncDisposable> SubscribeCoinPricesAsync(
        IReadOnlyList<string> coinIds,
        IReadOnlyList<string> vsCurrencies,
        Action<CoinPriceTick> onTick,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(coinIds);
        ArgumentNullException.ThrowIfNull(vsCurrencies);
        ArgumentNullException.ThrowIfNull(onTick);

        ValidateConnected();
        ValidateCapacity(_c1);

        var subscriberId = _c1.Subscribe(onTick);
        var dataJson = BuildSetTokensPayload("set_tokens", coinIds, vsCurrencies);

        await EnsureSubscribedAsync(C1Channel, ct).ConfigureAwait(false);

        await _sendLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await FrameSender.SendMessageAsync(_ws!, C1Channel, dataJson, ct).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }

        TrackSubscription(C1Channel, new RestorableSubscription(C1Channel, dataJson, subscriberId));

        return new SubscriptionHandle(async () =>
        {
            _c1.Unsubscribe(subscriberId);
            RemoveRestorableSubscription(C1Channel, subscriberId);

            if (_ws is { State: WebSocketState.Open })
            {
                var unsetJson = BuildSetTokensPayload("unset_tokens", coinIds, vsCurrencies);
                await _sendLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                try
                {
                    await FrameSender.SendMessageAsync(_ws, C1Channel, unsetJson, CancellationToken.None).ConfigureAwait(false);
                    if (_c1.Count == 0)
                    {
                        await FrameSender.SendUnsubscribeAsync(_ws, C1Channel, CancellationToken.None).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _sendLock.Release();
                }
            }
        });
    }

    /// <summary>G1: subscribe to onchain token prices. Tokens are <c>network_id:address</c> pairs.</summary>
    public async Task<IAsyncDisposable> SubscribeOnchainTokenPricesAsync(
        IReadOnlyList<string> networkAndTokenAddresses,
        Action<OnchainTokenPriceTick> onTick,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(networkAndTokenAddresses);
        ArgumentNullException.ThrowIfNull(onTick);

        ValidateConnected();
        ValidateCapacity(_g1);

        var subscriberId = _g1.Subscribe(onTick);
        var dataJson = BuildSetAddressesPayload("set_tokens", "tokens", networkAndTokenAddresses);

        await EnsureSubscribedAsync(G1Channel, ct).ConfigureAwait(false);

        await _sendLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await FrameSender.SendMessageAsync(_ws!, G1Channel, dataJson, ct).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }

        TrackSubscription(G1Channel, new RestorableSubscription(G1Channel, dataJson, subscriberId));

        return new SubscriptionHandle(async () =>
        {
            _g1.Unsubscribe(subscriberId);
            RemoveRestorableSubscription(G1Channel, subscriberId);

            if (_ws is { State: WebSocketState.Open })
            {
                var unsetJson = BuildSetAddressesPayload("unset_tokens", "tokens", networkAndTokenAddresses);
                await _sendLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                try
                {
                    await FrameSender.SendMessageAsync(_ws, G1Channel, unsetJson, CancellationToken.None).ConfigureAwait(false);
                    if (_g1.Count == 0)
                    {
                        await FrameSender.SendUnsubscribeAsync(_ws, G1Channel, CancellationToken.None).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _sendLock.Release();
                }
            }
        });
    }

    /// <summary>G2: subscribe to DEX pool trades. Pools are <c>network_id:pool_address</c> pairs.</summary>
    public async Task<IAsyncDisposable> SubscribeDexTradesAsync(
        IReadOnlyList<string> networkAndPoolAddresses,
        Action<DexTrade> onTrade,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(networkAndPoolAddresses);
        ArgumentNullException.ThrowIfNull(onTrade);

        ValidateConnected();
        ValidateCapacity(_g2);

        var subscriberId = _g2.Subscribe(onTrade);
        var dataJson = BuildSetAddressesPayload("set_pools", "pools", networkAndPoolAddresses);

        await EnsureSubscribedAsync(G2Channel, ct).ConfigureAwait(false);

        await _sendLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await FrameSender.SendMessageAsync(_ws!, G2Channel, dataJson, ct).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }

        TrackSubscription(G2Channel, new RestorableSubscription(G2Channel, dataJson, subscriberId));

        return new SubscriptionHandle(async () =>
        {
            _g2.Unsubscribe(subscriberId);
            RemoveRestorableSubscription(G2Channel, subscriberId);

            if (_ws is { State: WebSocketState.Open })
            {
                var unsetJson = BuildSetAddressesPayload("unset_pools", "pools", networkAndPoolAddresses);
                await _sendLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                try
                {
                    await FrameSender.SendMessageAsync(_ws, G2Channel, unsetJson, CancellationToken.None).ConfigureAwait(false);
                    if (_g2.Count == 0)
                    {
                        await FrameSender.SendUnsubscribeAsync(_ws, G2Channel, CancellationToken.None).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _sendLock.Release();
                }
            }
        });
    }

    /// <summary>G3: subscribe to DEX OHLCV candles.</summary>
    public async Task<IAsyncDisposable> SubscribeDexOhlcvAsync(
        IReadOnlyList<string> networkAndPoolAddresses,
        string interval,
        string token,
        Action<OnchainOhlcvCandle> onCandle,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(networkAndPoolAddresses);
        ArgumentNullException.ThrowIfNull(interval);
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(onCandle);

        ValidateConnected();
        ValidateCapacity(_g3);

        var subscriberId = _g3.Subscribe(onCandle);
        var dataJson = BuildOhlcvPayload("set_pools", networkAndPoolAddresses, interval, token);

        await EnsureSubscribedAsync(G3Channel, ct).ConfigureAwait(false);

        await _sendLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await FrameSender.SendMessageAsync(_ws!, G3Channel, dataJson, ct).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }

        TrackSubscription(G3Channel, new RestorableSubscription(G3Channel, dataJson, subscriberId));

        return new SubscriptionHandle(async () =>
        {
            _g3.Unsubscribe(subscriberId);
            RemoveRestorableSubscription(G3Channel, subscriberId);

            if (_ws is { State: WebSocketState.Open })
            {
                var unsetJson = BuildOhlcvPayload("unset_pools", networkAndPoolAddresses, interval, token);
                await _sendLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
                try
                {
                    await FrameSender.SendMessageAsync(_ws, G3Channel, unsetJson, CancellationToken.None).ConfigureAwait(false);
                    if (_g3.Count == 0)
                    {
                        await FrameSender.SendUnsubscribeAsync(_ws, G3Channel, CancellationToken.None).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _sendLock.Release();
                }
            }
        });
    }

    // ── DisposeAsync ─────────────────────────────────────────────────────────

    /// <summary>Dispose the stream, disconnecting gracefully if still connected.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        StopHeartbeatTimer();

        if (_receiveCts is not null)
        {
            await _receiveCts.CancelAsync().ConfigureAwait(false);
        }

        if (_receiveLoop is not null)
        {
            try
            {
                await _receiveLoop.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // best-effort
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        if (_ws is { State: WebSocketState.Open })
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.WsCloseException(_logger, ex);
            }
        }

        DisposeSocketSafe();
        _sendLock.Dispose();
        _stateLock.Dispose();
        _receiveCts?.Dispose();
    }

    // ── Internal: connection helpers ─────────────────────────────────────────

    private async Task OpenAndStartReceiveLoopAsync(CancellationToken ct)
    {
        _ws?.Dispose();
        _ws = new ClientWebSocket();

        if (!string.IsNullOrEmpty(_opts.ApiKey))
        {
            _ws.Options.SetRequestHeader("x-cg-pro-api-key", _opts.ApiKey);
        }

        _ws.Options.KeepAliveInterval = _opts.KeepAliveInterval;

        await _ws.ConnectAsync(_opts.BaseAddress, ct).ConfigureAwait(false);

        _receiveCts?.Dispose();
        _receiveCts = new CancellationTokenSource();
        _lastMessageAt = DateTimeOffset.UtcNow;
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), _receiveCts.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[_opts.ReceiveBufferSize];
        try
        {
            while (!ct.IsCancellationRequested && _ws!.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await _ws.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                _lastMessageAt = DateTimeOffset.UtcNow;
                ms.Position = 0;
                var frame = await JsonSerializer.DeserializeAsync(
                    ms,
                    ActionCableProtocolJsonContext.Default.ActionCableFrame,
                    ct).ConfigureAwait(false);
                if (frame is null)
                {
                    continue;
                }

                ProcessFrame(frame);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Caller cancelled — exit cleanly.
        }
        catch (Exception ex)
        {
            if (_opts.AutoReconnect)
            {
                _ = Task.Run(() => StartReconnectAsync(ex), CancellationToken.None);
            }
            else
            {
                await TransitionToAsync(StreamState.Faulted, ex).ConfigureAwait(false);
            }
        }
    }

    private void ProcessFrame(ActionCableFrame frame)
    {
        if (frame.Type is "ping" or "welcome" or "confirm_subscription")
        {
            return;
        }

        var channel = frame.Identifier?.Channel;
        if (channel is null)
        {
            return;
        }

        var message = frame.Message;
        if (message.ValueKind == JsonValueKind.Undefined)
        {
            return;
        }

        try
        {
            switch (channel)
            {
                case C1Channel:
                {
                    var tick = message.Deserialize(TicksJsonContext.Default.CoinPriceTick);
                    if (tick is not null)
                    {
                        _c1.Dispatch(tick);
                    }

                    break;
                }

                case G1Channel:
                {
                    var tick = message.Deserialize(TicksJsonContext.Default.OnchainTokenPriceTick);
                    if (tick is not null)
                    {
                        _g1.Dispatch(tick);
                    }

                    break;
                }

                case G2Channel:
                {
                    var trade = message.Deserialize(TicksJsonContext.Default.DexTrade);
                    if (trade is not null)
                    {
                        _g2.Dispatch(trade);
                    }

                    break;
                }

                case G3Channel:
                {
                    var candle = message.Deserialize(TicksJsonContext.Default.OnchainOhlcvCandle);
                    if (candle is not null)
                    {
                        _g3.Dispatch(candle);
                    }

                    break;
                }

                default:
                    Log.UnknownChannel(_logger, channel);
                    break;
            }
        }
        catch (JsonException ex)
        {
            Log.DeserializeFailed(_logger, channel, ex);
        }
    }

    private async Task TransitionToAsync(StreamState next, Exception? error = null)
    {
        await _stateLock.WaitAsync().ConfigureAwait(false);
        StreamState previous;
        try
        {
            previous = State;
            State = next;
            if (error is not null)
            {
                LastException = error;
            }
        }
        finally
        {
            _stateLock.Release();
        }

        Log.StateTransition(_logger, previous, next);

        if (next == StreamState.Connected)
        {
            StartHeartbeatTimer();
        }
        else if (previous == StreamState.Connected)
        {
            StopHeartbeatTimer();
        }

        StateChanged?.Invoke(this, new StreamStateChangedEventArgs(previous, next, error));
    }

    private async Task EnsureSubscribedAsync(string channel, CancellationToken ct)
    {
        bool isFirstSubscriber;
        lock (_activeSubs)
        {
            isFirstSubscriber = !_activeSubs.ContainsKey(channel) || _activeSubs[channel].Count == 0;
        }

        if (isFirstSubscriber)
        {
            await _sendLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                await FrameSender.SendSubscribeAsync(_ws!, channel, ct).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }

    private void TrackSubscription(string channel, RestorableSubscription subscription)
    {
        lock (_activeSubs)
        {
            if (!_activeSubs.TryGetValue(channel, out var list))
            {
                list = new List<RestorableSubscription>();
                _activeSubs[channel] = list;
            }

            list.Add(subscription);
        }
    }

    private void RemoveRestorableSubscription(string channel, Guid subscriberId)
    {
        lock (_activeSubs)
        {
            if (_activeSubs.TryGetValue(channel, out var list))
            {
                list.RemoveAll(s => s.SubscriberId == subscriberId);
                if (list.Count == 0)
                {
                    _activeSubs.Remove(channel);
                }
            }
        }
    }

    private void ValidateConnected()
    {
        if (State != StreamState.Connected)
        {
            throw new CoinGeckoStreamException(
                $"Cannot subscribe while stream is in state {State}. Call ConnectAsync first.");
        }
    }

    private void ValidateCapacity<T>(ChannelDispatcher<T> dispatcher)
    {
        if (dispatcher.Count >= _opts.MaxSubscriptionsPerChannel)
        {
            throw new CoinGeckoStreamException(
                $"Maximum subscriptions per channel ({_opts.MaxSubscriptionsPerChannel}) reached.");
        }
    }

    private void DisposeSocketSafe()
    {
        try
        {
            _ws?.Dispose();
        }
        catch (Exception ex)
        {
            Log.WsCloseException(_logger, ex);
        }

        _ws = null;
    }

    // ── JSON payload builders ────────────────────────────────────────────────

    private static string BuildSetTokensPayload(string action, IReadOnlyList<string> coinIds, IReadOnlyList<string> vsCurrencies)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteStartObject();
        writer.WriteString("action", action);
        writer.WriteStartArray("tokens");
        foreach (var id in coinIds)
        {
            writer.WriteStringValue(id);
        }

        writer.WriteEndArray();
        writer.WriteStartArray("currencies");
        foreach (var vs in vsCurrencies)
        {
            writer.WriteStringValue(vs);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static string BuildSetAddressesPayload(string action, string arrayKey, IReadOnlyList<string> addresses)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteStartObject();
        writer.WriteString("action", action);
        writer.WriteStartArray(arrayKey);
        foreach (var addr in addresses)
        {
            writer.WriteStringValue(addr);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static string BuildOhlcvPayload(
        string action,
        IReadOnlyList<string> addresses,
        string interval,
        string token)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteStartObject();
        writer.WriteString("action", action);
        writer.WriteStartArray("pools");
        foreach (var addr in addresses)
        {
            writer.WriteStringValue(addr);
        }

        writer.WriteEndArray();
        writer.WriteString("interval", interval);
        writer.WriteString("token", token);
        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    // ── Phase 6: Reconnect + heartbeat ──────────────────────────────────────

    private async Task StartReconnectAsync(Exception cause)
    {
        await TransitionToAsync(StreamState.Reconnecting, cause).ConfigureAwait(false);
        DisposeSocketSafe();

        for (var attempt = 1; attempt <= _opts.MaxReconnectAttempts; attempt++)
        {
            _reconnectAttempts = attempt;
            var delay = ComputeBackoff(attempt);
            try
            {
                await Task.Delay(delay).ConfigureAwait(false);
                await OpenAndStartReceiveLoopAsync(CancellationToken.None).ConfigureAwait(false);
                await RestoreSubscriptionsAsync(CancellationToken.None).ConfigureAwait(false);
                await TransitionToAsync(StreamState.Connected, null).ConfigureAwait(false);
                _reconnectAttempts = 0;
                return;
            }
            catch (Exception ex)
            {
                Log.ReconnectAttemptFailed(_logger, attempt, ex);
            }
        }

        await TransitionToAsync(StreamState.Faulted, cause).ConfigureAwait(false);
    }

    private static TimeSpan ComputeBackoff(int attempt)
    {
        const int baseMs = 1000;
        const int cap = 30_000;
        var previousMs = Math.Min(cap, baseMs * (int)Math.Pow(2, Math.Min(10, attempt - 1)));
        var next = Random.Shared.Next(baseMs, Math.Max(baseMs + 1, previousMs * 3));
        return TimeSpan.FromMilliseconds(Math.Min(cap, next));
    }

    private async Task RestoreSubscriptionsAsync(CancellationToken ct)
    {
        KeyValuePair<string, List<RestorableSubscription>>[] snapshot;
        lock (_activeSubs)
        {
            snapshot = _activeSubs.ToArray();
        }

        foreach (var (channel, subs) in snapshot)
        {
            await FrameSender.SendSubscribeAsync(_ws!, channel, ct).ConfigureAwait(false);
            foreach (var sub in subs)
            {
                await FrameSender.SendMessageAsync(_ws!, channel, sub.DataJson, ct).ConfigureAwait(false);
            }
        }
    }

    private void StartHeartbeatTimer()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = new Timer(_ =>
        {
            if (State != StreamState.Connected)
            {
                return;
            }

            var elapsed = DateTimeOffset.UtcNow - _lastMessageAt;
            if (elapsed > _opts.HeartbeatTimeout)
            {
                _ = Task.Run(() => StartReconnectAsync(
                    new CoinGeckoStreamException($"Heartbeat timeout after {elapsed}.")));
            }
        }, state: null, dueTime: _opts.HeartbeatTimeout / 4, period: _opts.HeartbeatTimeout / 4);
    }

    private void StopHeartbeatTimer()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    // ── Log message definitions (CA1848) ─────────────────────────────────────

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "State transition: {Previous} → {Next}")]
        public static partial void StateTransition(ILogger logger, StreamState previous, StreamState next);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Unknown channel: {Channel}")]
        public static partial void UnknownChannel(ILogger logger, string channel);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Failed to deserialize push message for channel {Channel}.")]
        public static partial void DeserializeFailed(ILogger logger, string channel, Exception ex);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Exception during WebSocket close.")]
        public static partial void WsCloseException(ILogger logger, Exception ex);

        [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Reconnect attempt {Attempt} failed")]
        public static partial void ReconnectAttemptFailed(ILogger logger, int attempt, Exception ex);
    }
}

internal sealed record RestorableSubscription(string Channel, string DataJson, Guid SubscriberId);
