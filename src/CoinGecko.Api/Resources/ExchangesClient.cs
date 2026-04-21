namespace CoinGecko.Api.Resources;

internal sealed class ExchangesClient(HttpClient http) : IExchangesClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
