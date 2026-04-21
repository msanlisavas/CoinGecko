namespace CoinGecko.Api.Resources;

internal sealed class OnchainClient(HttpClient http) : IOnchainClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
