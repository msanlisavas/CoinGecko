namespace CoinGecko.Api.Resources;

internal sealed class CoinsClient(HttpClient http) : ICoinsClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
