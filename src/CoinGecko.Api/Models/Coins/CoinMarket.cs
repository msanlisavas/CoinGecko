using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>A row from <c>/coins/markets</c>.</summary>
public sealed class CoinMarket
{
    /// <summary>CoinGecko id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Icon URL.</summary>
    [JsonPropertyName("image")] public string? Image { get; init; }
    /// <summary>Current price in the requested vs_currency.</summary>
    [JsonPropertyName("current_price")] public decimal? CurrentPrice { get; init; }
    /// <summary>Market cap in the requested vs_currency.</summary>
    [JsonPropertyName("market_cap")] public decimal? MarketCap { get; init; }
    /// <summary>Market-cap rank at snapshot time.</summary>
    [JsonPropertyName("market_cap_rank")] public int? MarketCapRank { get; init; }
    /// <summary>Fully-diluted valuation.</summary>
    [JsonPropertyName("fully_diluted_valuation")] public decimal? FullyDilutedValuation { get; init; }
    /// <summary>Total 24h trading volume.</summary>
    [JsonPropertyName("total_volume")] public decimal? TotalVolume { get; init; }
    /// <summary>24h high.</summary>
    [JsonPropertyName("high_24h")] public decimal? High24h { get; init; }
    /// <summary>24h low.</summary>
    [JsonPropertyName("low_24h")] public decimal? Low24h { get; init; }
    /// <summary>24h absolute price change.</summary>
    [JsonPropertyName("price_change_24h")] public decimal? PriceChange24h { get; init; }
    /// <summary>24h percentage price change.</summary>
    [JsonPropertyName("price_change_percentage_24h")] public decimal? PriceChangePercentage24h { get; init; }
    /// <summary>24h absolute market-cap change.</summary>
    [JsonPropertyName("market_cap_change_24h")] public decimal? MarketCapChange24h { get; init; }
    /// <summary>24h percentage market-cap change.</summary>
    [JsonPropertyName("market_cap_change_percentage_24h")] public decimal? MarketCapChangePercentage24h { get; init; }
    /// <summary>Circulating supply.</summary>
    [JsonPropertyName("circulating_supply")] public decimal? CirculatingSupply { get; init; }
    /// <summary>Total supply (may be null for inflationary coins).</summary>
    [JsonPropertyName("total_supply")] public decimal? TotalSupply { get; init; }
    /// <summary>Maximum supply.</summary>
    [JsonPropertyName("max_supply")] public decimal? MaxSupply { get; init; }
    /// <summary>All-time high.</summary>
    [JsonPropertyName("ath")] public decimal? Ath { get; init; }
    /// <summary>ATH change percentage.</summary>
    [JsonPropertyName("ath_change_percentage")] public decimal? AthChangePercentage { get; init; }
    /// <summary>ATH date (ISO-8601).</summary>
    [JsonPropertyName("ath_date")] public DateTimeOffset? AthDate { get; init; }
    /// <summary>All-time low.</summary>
    [JsonPropertyName("atl")] public decimal? Atl { get; init; }
    /// <summary>ATL change percentage.</summary>
    [JsonPropertyName("atl_change_percentage")] public decimal? AtlChangePercentage { get; init; }
    /// <summary>ATL date.</summary>
    [JsonPropertyName("atl_date")] public DateTimeOffset? AtlDate { get; init; }
    /// <summary>Last updated timestamp.</summary>
    [JsonPropertyName("last_updated")] public DateTimeOffset? LastUpdated { get; init; }
    /// <summary>Sparkline data (only when <c>sparkline=true</c>).</summary>
    [JsonPropertyName("sparkline_in_7d")] public SparklineIn7d? SparklineIn7d { get; init; }
    /// <summary>Percentage change by requested windows, e.g. <c>price_change_percentage_24h_in_currency</c>. Null when not requested.</summary>
    [JsonExtensionData] public IDictionary<string, System.Text.Json.JsonElement>? ExtraFields { get; set; }
}

/// <summary>7-day sparkline data.</summary>
public sealed class SparklineIn7d
{
    /// <summary>Hourly prices over the last 7 days.</summary>
    [JsonPropertyName("price")] public IReadOnlyList<decimal>? Price { get; init; }
}
