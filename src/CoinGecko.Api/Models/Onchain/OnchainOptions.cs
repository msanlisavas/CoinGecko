namespace CoinGecko.Api.Models.Onchain;

/// <summary>Common include-list + page options for single-pool lookups.</summary>
public sealed record OnchainPoolOptions
{
    /// <summary>Resources to include (e.g. <c>["base_token","quote_token","dex"]</c>).</summary>
    public IReadOnlyList<string>? Include { get; init; }
}

/// <summary>Options for trending-pools endpoints.</summary>
public sealed record OnchainTrendingPoolsOptions
{
    /// <summary>Include related resources.</summary>
    public IReadOnlyList<string>? Include { get; init; }
    /// <summary>Page (1-indexed).</summary>
    public int Page { get; init; } = 1;
    /// <summary>Trending duration window (<c>"5m"</c>, <c>"1h"</c>, <c>"6h"</c>, <c>"24h"</c>).</summary>
    public string? Duration { get; init; }
}

/// <summary>Paginated list options for pool endpoints.</summary>
public sealed record OnchainPoolsListOptions
{
    /// <summary>Include related resources.</summary>
    public IReadOnlyList<string>? Include { get; init; }
    /// <summary>Page (1-indexed).</summary>
    public int Page { get; init; } = 1;
    /// <summary>Sort key (e.g. <c>"h24_volume_usd_desc"</c>).</summary>
    public string? Sort { get; init; }
}

/// <summary>Megafilter options (Analyst+).</summary>
public sealed record OnchainMegafilterOptions
{
    /// <summary>Networks (CSV).</summary>
    public IReadOnlyList<string>? Networks { get; init; }
    /// <summary>DEXes.</summary>
    public IReadOnlyList<string>? Dexes { get; init; }
    /// <summary>Categories.</summary>
    public IReadOnlyList<string>? Categories { get; init; }
    /// <summary>Pool creation window (<c>"1h"</c>, <c>"24h"</c>, etc.).</summary>
    public string? PoolCreatedHour { get; init; }
    /// <summary>Minimum 24h volume USD.</summary>
    public decimal? MinVolumeH24Usd { get; init; }
    /// <summary>Minimum reserve USD.</summary>
    public decimal? MinReserveInUsd { get; init; }
    /// <summary>Sort key.</summary>
    public string? Sort { get; init; }
    /// <summary>Page.</summary>
    public int Page { get; init; } = 1;
}

/// <summary>Options for the OHLCV endpoint.</summary>
public sealed record OnchainOhlcvOptions
{
    /// <summary>Aggregate (1, 4, 12 for hour; 1, 7, 14, 30 for day; etc.).</summary>
    public int? Aggregate { get; init; }
    /// <summary>Limit (1–1000).</summary>
    public int? Limit { get; init; }
    /// <summary>Before timestamp (UNIX seconds).</summary>
    public DateTimeOffset? BeforeTimestamp { get; init; }
    /// <summary>Quote currency override (<c>"usd"</c>, <c>"token"</c>).</summary>
    public string? Currency { get; init; }
    /// <summary>Token choice (<c>"base"</c>, <c>"quote"</c>).</summary>
    public string? Token { get; init; }
}

/// <summary>Options for the top-holders endpoint.</summary>
public sealed record OnchainTopHoldersOptions
{
    /// <summary>How many top holders to return. Pass an integer (max 50, 40 for Solana) or <c>"max"</c>. Defaults to 10 server-side.</summary>
    public string? Holders { get; init; }
    /// <summary>If true, include average buy price, realized/unrealized PnL, and per-holder transaction counts in the response.</summary>
    public bool? IncludePnlDetails { get; init; }
}

/// <summary>Timeframe granularity for OHLCV.</summary>
public enum OnchainTimeframe
{
    /// <summary>Daily bars.</summary>
    Day,
    /// <summary>Hourly bars.</summary>
    Hour,
    /// <summary>Minute bars.</summary>
    Minute,
}
