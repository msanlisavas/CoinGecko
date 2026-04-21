namespace CoinGecko.Api.Resources;

internal sealed class CategoriesClient(HttpClient http) : ICategoriesClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
