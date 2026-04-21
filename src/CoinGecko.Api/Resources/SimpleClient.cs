namespace CoinGecko.Api.Resources;

internal sealed class SimpleClient(HttpClient http) : ISimpleClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
