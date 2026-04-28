using System.Text.Json.Serialization;
using CoinGecko.Api.Serialization.JsonApi;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>A liquidity pool on a DEX.</summary>
public sealed class Pool
{
    /// <summary>Pool id (typically <c>{network}_{address}</c>).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type (always <c>"pool"</c>).</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Pool attributes.</summary>
    [JsonPropertyName("attributes")] public PoolAttributes? Attributes { get; init; }
    /// <summary>References to related resources (base_token, quote_token, dex, network).</summary>
    [JsonPropertyName("relationships")] public PoolRelationships? Relationships { get; init; }
}

/// <summary>Pool display data.</summary>
public sealed class PoolAttributes
{
    /// <summary>Pool name (e.g. <c>"WETH / USDC"</c>).</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Pool address on-chain.</summary>
    [JsonPropertyName("address")] public string? Address { get; init; }
    /// <summary>Base token price in USD.</summary>
    [JsonPropertyName("base_token_price_usd")] public decimal? BaseTokenPriceUsd { get; init; }
    /// <summary>Quote token price in USD.</summary>
    [JsonPropertyName("quote_token_price_usd")] public decimal? QuoteTokenPriceUsd { get; init; }
    /// <summary>Base token price in native asset.</summary>
    [JsonPropertyName("base_token_price_native_currency")] public decimal? BaseTokenPriceNativeCurrency { get; init; }
    /// <summary>Base token price in quote token units.</summary>
    [JsonPropertyName("base_token_price_quote_token")] public decimal? BaseTokenPriceQuoteToken { get; init; }
    /// <summary>Quote token price in base token units.</summary>
    [JsonPropertyName("quote_token_price_base_token")] public decimal? QuoteTokenPriceBaseToken { get; init; }
    /// <summary>Pool creation timestamp (ISO).</summary>
    [JsonPropertyName("pool_created_at")] public string? PoolCreatedAt { get; init; }
    /// <summary>Reserve in USD.</summary>
    [JsonPropertyName("reserve_in_usd")] public decimal? ReserveInUsd { get; init; }
    /// <summary>Fully diluted valuation USD.</summary>
    [JsonPropertyName("fdv_usd")] public decimal? FdvUsd { get; init; }
    /// <summary>Market cap in USD.</summary>
    [JsonPropertyName("market_cap_usd")] public decimal? MarketCapUsd { get; init; }
    /// <summary>Price change percentage over windows (m5, h1, h6, h24).</summary>
    [JsonPropertyName("price_change_percentage")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage { get; init; }
    /// <summary>Transactions broken down by window.</summary>
    [JsonPropertyName("transactions")] public IReadOnlyDictionary<string, IReadOnlyDictionary<string, long?>>? Transactions { get; init; }
    /// <summary>Volume (USD) per window.</summary>
    [JsonPropertyName("volume_usd")] public IReadOnlyDictionary<string, decimal?>? VolumeUsd { get; init; }
    /// <summary>True if the pool is verified by GeckoTerminal.</summary>
    [JsonPropertyName("gt_verified")] public bool? GtVerified { get; init; }
}

/// <summary>Relationship pointers for a pool.</summary>
public sealed class PoolRelationships
{
    /// <summary>Base token reference.</summary>
    [JsonPropertyName("base_token")] public JsonApiRelationship? BaseToken { get; init; }
    /// <summary>Quote token reference.</summary>
    [JsonPropertyName("quote_token")] public JsonApiRelationship? QuoteToken { get; init; }
    /// <summary>DEX reference.</summary>
    [JsonPropertyName("dex")] public JsonApiRelationship? Dex { get; init; }
    /// <summary>Network reference.</summary>
    [JsonPropertyName("network")] public JsonApiRelationship? Network { get; init; }
}
