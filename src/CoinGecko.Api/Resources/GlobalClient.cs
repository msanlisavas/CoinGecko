namespace CoinGecko.Api.Resources;

internal sealed class GlobalClient(HttpClient http) : IGlobalClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
