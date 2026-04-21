using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>Uniswap-compatible token list (schema per https://tokenlists.org/).</summary>
public sealed class TokenList
{
    /// <summary>List name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Logo URI.</summary>
    [JsonPropertyName("logoURI")] public string? LogoUri { get; init; }

    /// <summary>ISO-8601 timestamp.</summary>
    [JsonPropertyName("timestamp")] public string? Timestamp { get; init; }

    /// <summary>Semantic version (major/minor/patch).</summary>
    [JsonPropertyName("version")] public TokenListVersion? Version { get; init; }

    /// <summary>Free-form tags.</summary>
    [JsonPropertyName("keywords")] public IReadOnlyList<string>? Keywords { get; init; }

    /// <summary>Token entries.</summary>
    [JsonPropertyName("tokens")] public IReadOnlyList<TokenListItem>? Tokens { get; init; }
}

/// <summary>Token-list semantic version.</summary>
public sealed class TokenListVersion
{
    /// <summary>Major.</summary>
    [JsonPropertyName("major")] public int Major { get; init; }

    /// <summary>Minor.</summary>
    [JsonPropertyName("minor")] public int Minor { get; init; }

    /// <summary>Patch.</summary>
    [JsonPropertyName("patch")] public int Patch { get; init; }
}

/// <summary>One row in a token list.</summary>
public sealed class TokenListItem
{
    /// <summary>EVM chain id.</summary>
    [JsonPropertyName("chainId")] public int? ChainId { get; init; }

    /// <summary>Contract address (checksummed).</summary>
    [JsonPropertyName("address")] public string? Address { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }

    /// <summary>Decimals.</summary>
    [JsonPropertyName("decimals")] public int? Decimals { get; init; }

    /// <summary>Logo URI.</summary>
    [JsonPropertyName("logoURI")] public string? LogoUri { get; init; }
}
