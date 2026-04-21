namespace CoinGecko.Api.Resources;

internal sealed class CompaniesClient(HttpClient http) : ICompaniesClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
