namespace CoinGecko.Api.Resources;

internal sealed class PingClient(HttpClient http) : IPingClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 8.
}
