namespace CoinGecko.Api.Resources;

internal sealed class NftsClient(HttpClient http) : INftsClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
