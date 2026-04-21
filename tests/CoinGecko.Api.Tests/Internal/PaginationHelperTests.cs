using CoinGecko.Api.Internal;

namespace CoinGecko.Api.Tests.Internal;

public class PaginationHelperTests
{
    private static readonly int[] Page1 = { 1, 2, 3, 4, 5 };
    private static readonly int[] Page2 = { 6, 7, 8, 9, 10 };
    private static readonly int[] Page3 = { 11, 12 };
    private static readonly int[] ExpectedAll = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    private static readonly int[] CancelPage = { 1, 2, 3 };

    [Fact]
    public async Task Yields_across_pages_and_stops_on_short_page()
    {
        var pages = new List<int[]> { Page1, Page2, Page3 };

        var collected = new List<int>();
        await foreach (var n in PaginationHelper.EnumerateAsync(
            fetchPage: (page, ct) => Task.FromResult((IReadOnlyList<int>)pages[page - 1]),
            perPage: 5,
            ct: TestContext.Current.CancellationToken))
        {
            collected.Add(n);
        }

        collected.ShouldBe(ExpectedAll);
    }

    [Fact]
    public async Task Respects_cancellation_between_pages()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var enumerator = PaginationHelper.EnumerateAsync<int>(
            fetchPage: (_, _) =>
            {
                cts.Cancel();
                return Task.FromResult((IReadOnlyList<int>)CancelPage);
            },
            perPage: 3,
            ct: cts.Token).GetAsyncEnumerator(cts.Token);

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            while (await enumerator.MoveNextAsync()) { /* drain */ }
        });
    }
}
