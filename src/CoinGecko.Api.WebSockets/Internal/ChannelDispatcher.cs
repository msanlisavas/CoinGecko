using System.Collections.Concurrent;

namespace CoinGecko.Api.WebSockets.Internal;

/// <summary>Per-channel dispatcher: routes decoded push-messages to subscriber callbacks.</summary>
internal sealed class ChannelDispatcher<TTick>
{
    private readonly ConcurrentDictionary<Guid, Action<TTick>> _subscribers = new();

    public Guid Subscribe(Action<TTick> onTick)
    {
        var id = Guid.NewGuid();
        _subscribers[id] = onTick;
        return id;
    }

    public void Unsubscribe(Guid id) => _subscribers.TryRemove(id, out _);

    public int Count => _subscribers.Count;

    public void Dispatch(TTick tick)
    {
        foreach (var kvp in _subscribers)
        {
            try { kvp.Value(tick); } catch { /* subscriber exceptions must not kill the receive loop */ }
        }
    }
}
