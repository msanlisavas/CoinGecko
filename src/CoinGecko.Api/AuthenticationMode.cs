namespace CoinGecko.Api;

/// <summary>
/// How the API key is transmitted on each request. Header is recommended and default;
/// query-string exists for edge cases such as caching proxies that key off the URL.
/// </summary>
public enum AuthenticationMode
{
    /// <summary>Send the key via <c>x-cg-demo-api-key</c> / <c>x-cg-pro-api-key</c> header.</summary>
    Header = 0,

    /// <summary>Send the key via <c>x_cg_demo_api_key</c> / <c>x_cg_pro_api_key</c> query param.</summary>
    QueryString = 1,
}
