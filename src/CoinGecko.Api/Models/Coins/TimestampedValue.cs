namespace CoinGecko.Api.Models;

/// <summary>A (timestamp, value) pair from CoinGecko chart endpoints.</summary>
public sealed record TimestampedValue(DateTimeOffset Timestamp, decimal Value);
