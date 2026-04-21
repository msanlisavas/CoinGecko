namespace CoinGecko.Api.Models;

/// <summary>Options for <c>/coins/markets</c>.</summary>
public sealed record CoinMarketsOptions
{
    /// <summary>Filter to these coin ids.</summary>
    public IReadOnlyList<string>? Ids { get; init; }
    /// <summary>Filter to a specific category.</summary>
    public string? Category { get; init; }
    /// <summary>Sort order.</summary>
    public CoinMarketsOrder Order { get; init; } = CoinMarketsOrder.MarketCapDesc;
    /// <summary>Items per page (1–250, default 100).</summary>
    public int PerPage { get; init; } = 100;
    /// <summary>Page number (1-indexed).</summary>
    public int Page { get; init; } = 1;
    /// <summary>Include 7d sparkline data.</summary>
    public bool Sparkline { get; init; }
    /// <summary>Percentage-change windows to include in response.</summary>
    public IReadOnlyList<PriceChangeWindow>? PriceChangePercentage { get; init; }
    /// <summary>Response localization language code.</summary>
    public string? Locale { get; init; }
    /// <summary>Decimal precision for price fields.</summary>
    public string? Precision { get; init; }
}

/// <summary>Options for <c>/coins/{id}</c>.</summary>
public sealed record CoinDetailOptions
{
    /// <summary>Include localization data.</summary>
    public bool Localization { get; init; }
    /// <summary>Include embedded top tickers.</summary>
    public bool Tickers { get; init; } = true;
    /// <summary>Include market data.</summary>
    public bool MarketData { get; init; } = true;
    /// <summary>Include community data.</summary>
    public bool CommunityData { get; init; } = true;
    /// <summary>Include developer data.</summary>
    public bool DeveloperData { get; init; } = true;
    /// <summary>Include sparkline.</summary>
    public bool Sparkline { get; init; }
    /// <summary>Include category detail objects (verbose).</summary>
    public bool IncludeCategoriesDetails { get; init; }
    /// <summary>DEX pair format for embedded tickers.</summary>
    public string? DexPairFormat { get; init; }
}

/// <summary>Options for <c>/coins/{id}/tickers</c>.</summary>
public sealed record CoinTickersOptions
{
    /// <summary>Filter to specific exchange ids.</summary>
    public IReadOnlyList<string>? ExchangeIds { get; init; }
    /// <summary>Include exchange logo in tickers.</summary>
    public bool IncludeExchangeLogo { get; init; }
    /// <summary>Page (1-indexed).</summary>
    public int Page { get; init; } = 1;
    /// <summary>Sort order (e.g. "trust_score_desc").</summary>
    public string? Order { get; init; }
    /// <summary>Include order-book depth.</summary>
    public bool Depth { get; init; }
    /// <summary>DEX pair format override.</summary>
    public string? DexPairFormat { get; init; }
}
