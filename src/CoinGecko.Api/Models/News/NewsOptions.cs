namespace CoinGecko.Api.Models.News;

/// <summary>Query options for <c>GET /news</c>.</summary>
public sealed record NewsOptions
{
    /// <summary>1-indexed page (max 20).</summary>
    public int? Page { get; init; }
    /// <summary>Results per page (max 20). Default 10.</summary>
    public int? PerPage { get; init; }
    /// <summary>Filter to news/guides referencing this CoinGecko coin id.</summary>
    public string? CoinId { get; init; }
    /// <summary>Language code (<c>en</c>, <c>ja</c>, <c>tr</c>, …). Default <c>en</c>.</summary>
    public string? Language { get; init; }
    /// <summary>Item type filter — <c>all</c>, <c>news</c>, or <c>guides</c> (guides require <see cref="CoinId"/>). Default <c>all</c>.</summary>
    public string? Type { get; init; }
}
