namespace CoinGecko.Api.Models;

/// <summary>Options for <see cref="Resources.ISimpleClient.GetPriceAsync"/>.</summary>
public sealed record SimplePriceOptions
{
    /// <summary>Coin ids (mutually replaces <see cref="Names"/> / <see cref="Symbols"/>).</summary>
    public IReadOnlyList<string>? Ids { get; init; }
    /// <summary>Coin names (alternative to Ids).</summary>
    public IReadOnlyList<string>? Names { get; init; }
    /// <summary>Coin symbols (alternative to Ids).</summary>
    public IReadOnlyList<string>? Symbols { get; init; }
    /// <summary>Quote currencies (required; e.g. <c>["usd","eur"]</c>).</summary>
    public required IReadOnlyList<string> VsCurrencies { get; init; }
    /// <summary>Include per-coin market cap.</summary>
    public bool IncludeMarketCap { get; init; }
    /// <summary>Include 24h volume.</summary>
    public bool Include24hrVol { get; init; }
    /// <summary>Include 24h change percentage.</summary>
    public bool Include24hrChange { get; init; }
    /// <summary>Include last-updated timestamp.</summary>
    public bool IncludeLastUpdatedAt { get; init; }
    /// <summary>Decimal precision for price fields (1–18 or <c>"full"</c>).</summary>
    public string? Precision { get; init; }
}

/// <summary>Options for <see cref="Resources.ISimpleClient.GetTokenPriceAsync"/>.</summary>
public sealed record SimpleTokenPriceOptions
{
    /// <summary>Contract addresses (required).</summary>
    public required IReadOnlyList<string> ContractAddresses { get; init; }
    /// <summary>Quote currencies (required).</summary>
    public required IReadOnlyList<string> VsCurrencies { get; init; }
    /// <summary>Include per-coin market cap.</summary>
    public bool IncludeMarketCap { get; init; }
    /// <summary>Include 24h volume.</summary>
    public bool Include24hrVol { get; init; }
    /// <summary>Include 24h change percentage.</summary>
    public bool Include24hrChange { get; init; }
    /// <summary>Include last-updated timestamp.</summary>
    public bool IncludeLastUpdatedAt { get; init; }
    /// <summary>Decimal precision.</summary>
    public string? Precision { get; init; }
}
