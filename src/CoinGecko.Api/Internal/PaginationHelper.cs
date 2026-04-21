using System.Runtime.CompilerServices;

namespace CoinGecko.Api.Internal;

internal static class PaginationHelper
{
    public static async IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
        int perPage,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var page = 1;
        while (!ct.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
            var items = await fetchPage(page, ct).ConfigureAwait(false);
            foreach (var item in items)
            {
                yield return item;
            }

            if (items.Count < perPage)
            {
                yield break;
            }

            page++;
        }

        ct.ThrowIfCancellationRequested();
    }
}
