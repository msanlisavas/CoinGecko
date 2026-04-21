using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Envelope around the global market snapshot (actual data lives in <see cref="Data"/>).</summary>
public sealed class GlobalMarketEnvelope
{
    /// <summary>The actual global data payload.</summary>
    [JsonPropertyName("data")] public GlobalMarket? Data { get; init; }
}

/// <summary>Aggregate market snapshot across all tracked coins.</summary>
public sealed class GlobalMarket
{
    /// <summary>Total tracked cryptocurrencies.</summary>
    [JsonPropertyName("active_cryptocurrencies")] public int ActiveCryptocurrencies { get; init; }
    /// <summary>Upcoming ICOs.</summary>
    [JsonPropertyName("upcoming_icos")] public int UpcomingIcos { get; init; }
    /// <summary>Ongoing ICOs.</summary>
    [JsonPropertyName("ongoing_icos")] public int OngoingIcos { get; init; }
    /// <summary>Ended ICOs.</summary>
    [JsonPropertyName("ended_icos")] public int EndedIcos { get; init; }
    /// <summary>Number of exchanges tracked.</summary>
    [JsonPropertyName("markets")] public int Markets { get; init; }
    /// <summary>Total market cap keyed by quote currency.</summary>
    [JsonPropertyName("total_market_cap")] public IReadOnlyDictionary<string, decimal>? TotalMarketCap { get; init; }
    /// <summary>Total 24h volume keyed by quote currency.</summary>
    [JsonPropertyName("total_volume")] public IReadOnlyDictionary<string, decimal>? TotalVolume { get; init; }
    /// <summary>Per-coin market-cap percentage (BTC dominance etc.).</summary>
    [JsonPropertyName("market_cap_percentage")] public IReadOnlyDictionary<string, decimal>? MarketCapPercentage { get; init; }
    /// <summary>24h market-cap change in USD.</summary>
    [JsonPropertyName("market_cap_change_percentage_24h_usd")] public decimal? MarketCapChangePercentage24hUsd { get; init; }
    /// <summary>UNIX seconds of snapshot.</summary>
    [JsonPropertyName("updated_at")] public long UpdatedAt { get; init; }
}

/// <summary>Envelope around the DeFi snapshot.</summary>
public sealed class DefiGlobalEnvelope
{
    /// <summary>DeFi payload.</summary>
    [JsonPropertyName("data")] public DefiGlobal? Data { get; init; }
}

/// <summary>Aggregate DeFi market snapshot.</summary>
public sealed class DefiGlobal
{
    /// <summary>Total DeFi market cap in USD.</summary>
    [JsonPropertyName("defi_market_cap")] public string? DefiMarketCap { get; init; }
    /// <summary>Total Ethereum market cap in USD (for comparison).</summary>
    [JsonPropertyName("eth_market_cap")] public string? EthMarketCap { get; init; }
    /// <summary>DeFi-to-Eth ratio.</summary>
    [JsonPropertyName("defi_to_eth_ratio")] public string? DefiToEthRatio { get; init; }
    /// <summary>Total 24h DeFi volume in USD.</summary>
    [JsonPropertyName("trading_volume_24h")] public string? TradingVolume24h { get; init; }
    /// <summary>DeFi dominance percentage.</summary>
    [JsonPropertyName("defi_dominance")] public string? DefiDominance { get; init; }
    /// <summary>Top coin display name.</summary>
    [JsonPropertyName("top_coin_name")] public string? TopCoinName { get; init; }
    /// <summary>Top coin dominance percentage.</summary>
    [JsonPropertyName("top_coin_defi_dominance")] public decimal? TopCoinDefiDominance { get; init; }
}

/// <summary>Historical global market cap, one timestamp + value pair.</summary>
public sealed record GlobalMarketCapPoint
{
    /// <summary>UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; init; }
    /// <summary>Market cap in the requested vs_currency.</summary>
    public decimal MarketCap { get; init; }
    /// <summary>24h volume in the requested vs_currency.</summary>
    public decimal Volume24h { get; init; }
}
