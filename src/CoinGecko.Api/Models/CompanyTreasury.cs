using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Aggregated company treasury data for a coin.</summary>
public sealed class CompanyTreasury
{
    /// <summary>Total holdings across all companies (native units).</summary>
    [JsonPropertyName("total_holdings")] public decimal? TotalHoldings { get; init; }

    /// <summary>Total holdings value in USD.</summary>
    [JsonPropertyName("total_value_usd")] public decimal? TotalValueUsd { get; init; }

    /// <summary>Percentage of total market cap held by these companies.</summary>
    [JsonPropertyName("market_cap_dominance")] public decimal? MarketCapDominance { get; init; }

    /// <summary>List of companies and their holdings.</summary>
    [JsonPropertyName("companies")] public IReadOnlyList<Company> Companies { get; init; } = Array.Empty<Company>();
}

/// <summary>One company entry in the treasury list.</summary>
public sealed class Company
{
    /// <summary>Company display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Ticker symbol (often exchange:ticker).</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }

    /// <summary>Country of incorporation (ISO code or name).</summary>
    [JsonPropertyName("country")] public string? Country { get; init; }

    /// <summary>Total holdings in native coin units.</summary>
    [JsonPropertyName("total_holdings")] public decimal? TotalHoldings { get; init; }

    /// <summary>Total entry value in USD when purchased.</summary>
    [JsonPropertyName("total_entry_value_usd")] public decimal? TotalEntryValueUsd { get; init; }

    /// <summary>Total current value in USD.</summary>
    [JsonPropertyName("total_current_value_usd")] public decimal? TotalCurrentValueUsd { get; init; }

    /// <summary>Percentage of coin's total supply held by this company.</summary>
    [JsonPropertyName("percentage_of_total_supply")] public decimal? PercentageOfTotalSupply { get; init; }
}
