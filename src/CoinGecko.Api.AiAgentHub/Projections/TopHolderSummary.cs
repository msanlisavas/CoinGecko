namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact top-holder row for LLM consumption.</summary>
/// <param name="Rank">1-indexed rank.</param>
/// <param name="Address">Holder wallet address.</param>
/// <param name="Label">Human-readable label (exchange, treasury, etc.) when known.</param>
/// <param name="Amount">Token balance held.</param>
/// <param name="Percentage">Percentage of supply held.</param>
/// <param name="ValueUsd">USD value of the position.</param>
/// <param name="AverageBuyPriceUsd">Average cost basis in USD (only when PnL details requested).</param>
/// <param name="UnrealizedPnlUsd">Unrealized PnL in USD (only when PnL details requested).</param>
/// <param name="RealizedPnlUsd">Realized PnL in USD (only when PnL details requested).</param>
public sealed record TopHolderSummary(
    int? Rank,
    string Address,
    string? Label,
    decimal? Amount,
    decimal? Percentage,
    decimal? ValueUsd,
    decimal? AverageBuyPriceUsd,
    decimal? UnrealizedPnlUsd,
    decimal? RealizedPnlUsd);
