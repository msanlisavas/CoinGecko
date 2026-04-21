namespace CoinGecko.Api.Resources;

internal sealed class TrendingClient(HttpClient http) : ITrendingClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
