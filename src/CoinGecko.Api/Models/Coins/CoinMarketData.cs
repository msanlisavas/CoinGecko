using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Comprehensive market data for a coin from <c>/coins/{id}</c>.</summary>
public sealed class CoinMarketData
{
    /// <summary>Current price keyed by currency code.</summary>
    [JsonPropertyName("current_price")] public IReadOnlyDictionary<string, decimal?>? CurrentPrice { get; init; }
    /// <summary>Total value locked (DeFi) keyed by currency.</summary>
    [JsonPropertyName("total_value_locked")] public IReadOnlyDictionary<string, decimal?>? TotalValueLocked { get; init; }
    /// <summary>MCap to TVL ratio.</summary>
    [JsonPropertyName("mcap_to_tvl_ratio")] public decimal? McapToTvlRatio { get; init; }
    /// <summary>FDV to TVL ratio.</summary>
    [JsonPropertyName("fdv_to_tvl_ratio")] public decimal? FdvToTvlRatio { get; init; }
    /// <summary>ROI data.</summary>
    [JsonPropertyName("roi")] public System.Text.Json.JsonElement? Roi { get; init; }
    /// <summary>All-time high keyed by currency code.</summary>
    [JsonPropertyName("ath")] public IReadOnlyDictionary<string, decimal?>? Ath { get; init; }
    /// <summary>ATH change percentage keyed by currency code.</summary>
    [JsonPropertyName("ath_change_percentage")] public IReadOnlyDictionary<string, decimal?>? AthChangePercentage { get; init; }
    /// <summary>ATH date keyed by currency code.</summary>
    [JsonPropertyName("ath_date")] public IReadOnlyDictionary<string, string?>? AthDate { get; init; }
    /// <summary>All-time low keyed by currency code.</summary>
    [JsonPropertyName("atl")] public IReadOnlyDictionary<string, decimal?>? Atl { get; init; }
    /// <summary>ATL change percentage keyed by currency code.</summary>
    [JsonPropertyName("atl_change_percentage")] public IReadOnlyDictionary<string, decimal?>? AtlChangePercentage { get; init; }
    /// <summary>ATL date keyed by currency code.</summary>
    [JsonPropertyName("atl_date")] public IReadOnlyDictionary<string, string?>? AtlDate { get; init; }
    /// <summary>Market cap keyed by currency code.</summary>
    [JsonPropertyName("market_cap")] public IReadOnlyDictionary<string, decimal?>? MarketCap { get; init; }
    /// <summary>Market cap rank.</summary>
    [JsonPropertyName("market_cap_rank")] public int? MarketCapRank { get; init; }
    /// <summary>Fully diluted valuation keyed by currency code.</summary>
    [JsonPropertyName("fully_diluted_valuation")] public IReadOnlyDictionary<string, decimal?>? FullyDilutedValuation { get; init; }
    /// <summary>Market cap / FDV ratio.</summary>
    [JsonPropertyName("market_cap_fdv_ratio")] public decimal? MarketCapFdvRatio { get; init; }
    /// <summary>Total volume keyed by currency code.</summary>
    [JsonPropertyName("total_volume")] public IReadOnlyDictionary<string, decimal?>? TotalVolume { get; init; }
    /// <summary>24h high keyed by currency code.</summary>
    [JsonPropertyName("high_24h")] public IReadOnlyDictionary<string, decimal?>? High24h { get; init; }
    /// <summary>24h low keyed by currency code.</summary>
    [JsonPropertyName("low_24h")] public IReadOnlyDictionary<string, decimal?>? Low24h { get; init; }
    /// <summary>24h absolute price change keyed by currency code.</summary>
    [JsonPropertyName("price_change_24h")] public decimal? PriceChange24h { get; init; }
    /// <summary>24h percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_24h")] public decimal? PriceChangePercentage24h { get; init; }
    /// <summary>7d percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_7d")] public decimal? PriceChangePercentage7d { get; init; }
    /// <summary>14d percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_14d")] public decimal? PriceChangePercentage14d { get; init; }
    /// <summary>30d percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_30d")] public decimal? PriceChangePercentage30d { get; init; }
    /// <summary>60d percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_60d")] public decimal? PriceChangePercentage60d { get; init; }
    /// <summary>200d percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_200d")] public decimal? PriceChangePercentage200d { get; init; }
    /// <summary>1y percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_1y")] public decimal? PriceChangePercentage1y { get; init; }
    /// <summary>24h market cap change.</summary>
    [JsonPropertyName("market_cap_change_24h")] public decimal? MarketCapChange24h { get; init; }
    /// <summary>24h market cap percentage change.</summary>
    [JsonPropertyName("market_cap_change_percentage_24h")] public decimal? MarketCapChangePercentage24h { get; init; }
    /// <summary>24h price change keyed by currency code.</summary>
    [JsonPropertyName("price_change_24h_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChange24hInCurrency { get; init; }
    /// <summary>24h price change percentage keyed by currency code.</summary>
    [JsonPropertyName("price_change_percentage_24h_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage24hInCurrency { get; init; }
    /// <summary>7d price change percentage keyed by currency code.</summary>
    [JsonPropertyName("price_change_percentage_7d_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage7dInCurrency { get; init; }
    /// <summary>14d price change percentage keyed by currency code.</summary>
    [JsonPropertyName("price_change_percentage_14d_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage14dInCurrency { get; init; }
    /// <summary>30d price change percentage keyed by currency code.</summary>
    [JsonPropertyName("price_change_percentage_30d_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage30dInCurrency { get; init; }
    /// <summary>60d price change percentage keyed by currency code.</summary>
    [JsonPropertyName("price_change_percentage_60d_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage60dInCurrency { get; init; }
    /// <summary>200d price change percentage keyed by currency code.</summary>
    [JsonPropertyName("price_change_percentage_200d_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage200dInCurrency { get; init; }
    /// <summary>1y price change percentage keyed by currency code.</summary>
    [JsonPropertyName("price_change_percentage_1y_in_currency")] public IReadOnlyDictionary<string, decimal?>? PriceChangePercentage1yInCurrency { get; init; }
    /// <summary>Market cap change in 24h keyed by currency code.</summary>
    [JsonPropertyName("market_cap_change_24h_in_currency")] public IReadOnlyDictionary<string, decimal?>? MarketCapChange24hInCurrency { get; init; }
    /// <summary>Market cap change percentage in 24h keyed by currency code.</summary>
    [JsonPropertyName("market_cap_change_percentage_24h_in_currency")] public IReadOnlyDictionary<string, decimal?>? MarketCapChangePercentage24hInCurrency { get; init; }
    /// <summary>Total supply.</summary>
    [JsonPropertyName("total_supply")] public decimal? TotalSupply { get; init; }
    /// <summary>Maximum supply.</summary>
    [JsonPropertyName("max_supply")] public decimal? MaxSupply { get; init; }
    /// <summary>Circulating supply.</summary>
    [JsonPropertyName("circulating_supply")] public decimal? CirculatingSupply { get; init; }
    /// <summary>Outstanding token supply (max theoretical supply for OTV calculation).</summary>
    [JsonPropertyName("outstanding_supply")] public decimal? OutstandingSupply { get; init; }
    /// <summary>Outstanding token value in USD — theoretical maximum valuation.</summary>
    [JsonPropertyName("outstanding_token_value_usd")] public decimal? OutstandingTokenValueUsd { get; init; }
    /// <summary>Last updated timestamp.</summary>
    [JsonPropertyName("last_updated")] public DateTimeOffset? LastUpdated { get; init; }
}
