namespace CoinGecko.Api.Resources;

internal sealed class SearchClient(HttpClient http) : ISearchClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
