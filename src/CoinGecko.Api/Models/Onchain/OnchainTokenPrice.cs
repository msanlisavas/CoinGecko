using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>Simple token-price response.</summary>
public sealed class OnchainTokenPrices
{
    /// <summary>Resource id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Price map attributes.</summary>
    [JsonPropertyName("attributes")] public OnchainTokenPricesAttributes? Attributes { get; init; }
}

/// <summary>Prices keyed by contract address.</summary>
public sealed class OnchainTokenPricesAttributes
{
    /// <summary>Token prices (key = contract address, value = price in the requested currency).</summary>
    [JsonPropertyName("token_prices")] public IReadOnlyDictionary<string, decimal?>? TokenPrices { get; init; }
}

/// <summary>Options for <c>/simple/networks/{network}/token_price/{addresses}</c>.</summary>
public sealed record OnchainTokenPriceOptions
{
    /// <summary>Include market cap.</summary>
    public bool IncludeMarketCap { get; init; }
    /// <summary>Include 24h volume.</summary>
    public bool Include24hrVol { get; init; }
    /// <summary>Include 24h change.</summary>
    public bool Include24hrPriceChange { get; init; }
    /// <summary>Include total reserve in USD.</summary>
    public bool IncludeTotalReserveInUsd { get; init; }
}
