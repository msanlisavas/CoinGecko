using System.Text.Json.Serialization;
using CoinGecko.Api.Serialization.JsonApi;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>A token on an onchain network.</summary>
public sealed class OnchainToken
{
    /// <summary>Token id (typically <c>{network}_{address}</c>).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Token attributes.</summary>
    [JsonPropertyName("attributes")] public OnchainTokenAttributes? Attributes { get; init; }
    /// <summary>Relationships (e.g. <c>top_pools</c>).</summary>
    [JsonPropertyName("relationships")] public OnchainTokenRelationships? Relationships { get; init; }
}

/// <summary>Token display / trading data.</summary>
public sealed class OnchainTokenAttributes
{
    /// <summary>Token address on-chain.</summary>
    [JsonPropertyName("address")] public string? Address { get; init; }
    /// <summary>Token display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Token symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Decimals.</summary>
    [JsonPropertyName("decimals")] public int? Decimals { get; init; }
    /// <summary>Image URL.</summary>
    [JsonPropertyName("image_url")] public string? ImageUrl { get; init; }
    /// <summary>CoinGecko coin id (if mapped).</summary>
    [JsonPropertyName("coingecko_coin_id")] public string? CoingeckoCoinId { get; init; }
    /// <summary>Total supply.</summary>
    [JsonPropertyName("total_supply")] public decimal? TotalSupply { get; init; }
    /// <summary>Normalized total supply.</summary>
    [JsonPropertyName("normalized_total_supply")] public decimal? NormalizedTotalSupply { get; init; }
    /// <summary>Price in USD.</summary>
    [JsonPropertyName("price_usd")] public decimal? PriceUsd { get; init; }
    /// <summary>FDV in USD.</summary>
    [JsonPropertyName("fdv_usd")] public decimal? FdvUsd { get; init; }
    /// <summary>Market cap in USD.</summary>
    [JsonPropertyName("market_cap_usd")] public decimal? MarketCapUsd { get; init; }
    /// <summary>24h trading volume in USD.</summary>
    [JsonPropertyName("volume_usd")] public IReadOnlyDictionary<string, decimal?>? VolumeUsd { get; init; }
    /// <summary>Total reserve across pools (USD).</summary>
    [JsonPropertyName("total_reserve_in_usd")] public decimal? TotalReserveInUsd { get; init; }
    /// <summary>Outstanding token supply (max theoretical supply for OTV calculation).</summary>
    [JsonPropertyName("outstanding_supply")] public decimal? OutstandingSupply { get; init; }
    /// <summary>Outstanding token value in USD — theoretical maximum valuation (price × outstanding_supply).</summary>
    [JsonPropertyName("outstanding_token_value_usd")] public decimal? OutstandingTokenValueUsd { get; init; }
    /// <summary>True if the token is verified by GeckoTerminal.</summary>
    [JsonPropertyName("gt_verified")] public bool? GtVerified { get; init; }
}

/// <summary>Token relationship pointers.</summary>
public sealed class OnchainTokenRelationships
{
    /// <summary>Top pools referenced by id.</summary>
    [JsonPropertyName("top_pools")] public JsonApiRelationship? TopPools { get; init; }
}
