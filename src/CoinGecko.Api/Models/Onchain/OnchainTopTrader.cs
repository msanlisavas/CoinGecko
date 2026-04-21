using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

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

/// <summary>A top holder entry.</summary>
public sealed class OnchainTopHolder
{
    /// <summary>Holder id (wallet address).</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Resource type.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>Attributes (address, balance, percentage).</summary>
    [JsonPropertyName("attributes")] public JsonObject? Attributes { get; init; }
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
