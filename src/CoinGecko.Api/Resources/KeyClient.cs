namespace CoinGecko.Api.Resources;

internal sealed class KeyClient(HttpClient http) : IKeyClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
