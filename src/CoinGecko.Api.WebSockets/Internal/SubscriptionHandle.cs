namespace CoinGecko.Api.WebSockets.Internal;

internal sealed class SubscriptionHandle(Func<ValueTask> onDispose) : IAsyncDisposable
{
    private int _disposed;

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return ValueTask.CompletedTask;
        }
        return onDispose();
    }
}
