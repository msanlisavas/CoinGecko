using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models.Onchain;

/// <summary>A top trader entry.</summary>
public sealed class OnchainTopTrader
{
    /// <summary>Trader id (wallet address).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Attributes (address, volume, pnl).</summary>
    [JsonPropertyName("attributes")] public JsonObject? Attributes { get; init; }
}

/// <summary>Top-holders response envelope from <c>/onchain/networks/{network}/tokens/{address}/top_holders</c>.</summary>
public sealed class OnchainTopHolders
{
    /// <summary>Resource id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>The list of holders plus a last-updated timestamp.</summary>
    [JsonPropertyName("attributes")] public OnchainTopHoldersAttributes? Attributes { get; init; }
}

/// <summary>Inner payload for the top-holders envelope.</summary>
public sealed class OnchainTopHoldersAttributes
{
    /// <summary>Last updated timestamp (ISO-8601).</summary>
    [JsonPropertyName("last_updated_at")] public string? LastUpdatedAt { get; init; }
    /// <summary>Top-holder rows, ordered by rank.</summary>
    [JsonPropertyName("holders")] public IReadOnlyList<OnchainTopHolder>? Holders { get; init; }
}

/// <summary>A single top-holder row.</summary>
/// <remarks>
/// PnL fields (<see cref="UnrealizedPnlUsd"/>, <see cref="UnrealizedPnlPercentage"/>,
/// <see cref="RealizedPnlUsd"/>, <see cref="RealizedPnlPercentage"/>, <see cref="AverageBuyPriceUsd"/>,
/// <see cref="TotalBuyCount"/>, <see cref="TotalSellCount"/>) are populated only when
/// <see cref="OnchainTopHoldersOptions.IncludePnlDetails"/> is set to <c>true</c>.
/// </remarks>
public sealed class OnchainTopHolder
{
    /// <summary>1-indexed rank.</summary>
    [JsonPropertyName("rank")] public int? Rank { get; init; }
    /// <summary>Holder wallet address.</summary>
    [JsonPropertyName("address")] public string? Address { get; init; }
    /// <summary>Optional human-readable label (e.g. exchange/treasury).</summary>
    [JsonPropertyName("label")] public string? Label { get; init; }
    /// <summary>Token amount held.</summary>
    [JsonPropertyName("amount")] public decimal? Amount { get; init; }
    /// <summary>Percentage of supply held.</summary>
    [JsonPropertyName("percentage")] public decimal? Percentage { get; init; }
    /// <summary>USD value of the position.</summary>
    [JsonPropertyName("value")] public decimal? Value { get; init; }
    /// <summary>Average buy price in USD (PnL detail).</summary>
    [JsonPropertyName("average_buy_price_usd")] public decimal? AverageBuyPriceUsd { get; init; }
    /// <summary>Total number of buy transactions (PnL detail).</summary>
    [JsonPropertyName("total_buy_count")] public long? TotalBuyCount { get; init; }
    /// <summary>Total number of sell transactions (PnL detail).</summary>
    [JsonPropertyName("total_sell_count")] public long? TotalSellCount { get; init; }
    /// <summary>Unrealized PnL in USD (PnL detail).</summary>
    [JsonPropertyName("unrealized_pnl_usd")] public decimal? UnrealizedPnlUsd { get; init; }
    /// <summary>Unrealized PnL as a percentage of cost basis (PnL detail).</summary>
    [JsonPropertyName("unrealized_pnl_percentage")] public decimal? UnrealizedPnlPercentage { get; init; }
    /// <summary>Realized PnL in USD (PnL detail).</summary>
    [JsonPropertyName("realized_pnl_usd")] public decimal? RealizedPnlUsd { get; init; }
    /// <summary>Realized PnL as a percentage (PnL detail).</summary>
    [JsonPropertyName("realized_pnl_percentage")] public decimal? RealizedPnlPercentage { get; init; }
    /// <summary>Block-explorer URL for the holder address.</summary>
    [JsonPropertyName("explorer_url")] public string? ExplorerUrl { get; init; }
}

/// <summary>Historical holder count for a token.</summary>
public sealed class OnchainHoldersChart
{
    /// <summary>Resource id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Holder-count time series.</summary>
    [JsonPropertyName("attributes")] public OnchainHoldersChartAttributes? Attributes { get; init; }
}

/// <summary>Holder-count series.</summary>
public sealed class OnchainHoldersChartAttributes
{
    /// <summary>[timestamp_seconds, holder_count] array.</summary>
    [JsonPropertyName("token_holders_list")] public decimal[][]? TokenHoldersList { get; init; }
}
