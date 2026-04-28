using System.Net.Http.Json;
using CoinGecko.Api.Internal;
using CoinGecko.Api.Models.News;
using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Resources;

internal sealed class NewsClient(HttpClient http) : INewsClient
{
    private readonly HttpClient _http = http;

    public async Task<NewsArticle[]> GetNewsAsync(NewsOptions? options = null, CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("page", options?.Page)
            .Add("per_page", options?.PerPage)
            .Add("coin_id", options?.CoinId)
            .Add("language", options?.Language)
            .Add("type", options?.Type);
        using var req = new HttpRequestMessage(HttpMethod.Get, "news" + qs);
        req.Options.Set(CoinGeckoRequestOptions.RequiredPlan, (CoinGeckoPlan?)CoinGeckoPlan.Analyst);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync(CoinGeckoJsonContext.Default.NewsArticleArray, ct).ConfigureAwait(false);
        return dto ?? Array.Empty<NewsArticle>();
    }
}
