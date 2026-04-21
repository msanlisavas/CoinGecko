namespace CoinGecko.Api.Models.Onchain;

/// <summary>Options for <c>/categories</c>.</summary>
public sealed record OnchainCategoriesOptions
{
    /// <summary>Page.</summary>
    public int Page { get; init; } = 1;
    /// <summary>Sort key.</summary>
    public string? Sort { get; init; }
}
