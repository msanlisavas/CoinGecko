using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Response from the <c>/ping</c> endpoint. Used to verify API reachability and credentials.</summary>
public sealed class PingResponse
{
    /// <summary>The welcome string CoinGecko returns (e.g. <c>"(V3) To the Moon!"</c>).</summary>
    [JsonPropertyName("gecko_says")] public string? GeckoSays { get; init; }
}
