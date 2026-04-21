namespace CoinGecko.Api.Models;

/// <summary>A (timestamp, supply) pair from CoinGecko supply-chart endpoints.</summary>
public sealed record SupplyPoint(DateTimeOffset Timestamp, decimal Supply);
