namespace CoinGecko.Api.Resources;

internal sealed class AssetPlatformsClient(HttpClient http) : IAssetPlatformsClient
{
    private readonly HttpClient _http = http;
    // Methods added in Phase 9.
}
