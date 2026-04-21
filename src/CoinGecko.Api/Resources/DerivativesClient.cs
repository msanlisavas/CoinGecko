namespace CoinGecko.Api.Resources;

internal sealed class DerivativesClient(HttpClient http) : IDerivativesClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
