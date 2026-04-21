using System.Text.Json.Serialization;

namespace CoinGecko.Api.Models;

/// <summary>NFT collection id-map entry.</summary>
public sealed class NftListItem
{
    /// <summary>NFT id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Contract address (for EVM chains).</summary>
    [JsonPropertyName("contract_address")] public string? ContractAddress { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Asset platform id.</summary>
    [JsonPropertyName("asset_platform_id")] public string? AssetPlatformId { get; init; }
    /// <summary>Ticker symbol (not always set for NFTs).</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
}

/// <summary>Full NFT collection detail.</summary>
public class Nft
{
    /// <summary>NFT id.</summary>
    [JsonPropertyName("id")] public string? Id { get; init; }
    /// <summary>Contract address.</summary>
    [JsonPropertyName("contract_address")] public string? ContractAddress { get; init; }
    /// <summary>Asset platform id.</summary>
    [JsonPropertyName("asset_platform_id")] public string? AssetPlatformId { get; init; }
    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Ticker symbol.</summary>
    [JsonPropertyName("symbol")] public string? Symbol { get; init; }
    /// <summary>HTML description.</summary>
    [JsonPropertyName("description")] public string? Description { get; init; }
    /// <summary>Native currency (e.g. <c>"eth"</c>).</summary>
    [JsonPropertyName("native_currency")] public string? NativeCurrency { get; init; }
    /// <summary>Native currency symbol (e.g. <c>"ETH"</c>).</summary>
    [JsonPropertyName("native_currency_symbol")] public string? NativeCurrencySymbol { get; init; }
    /// <summary>Floor price in native currency + USD.</summary>
    [JsonPropertyName("floor_price")] public IReadOnlyDictionary<string, decimal?>? FloorPrice { get; init; }
    /// <summary>Market capitalization (native + USD).</summary>
    [JsonPropertyName("market_cap")] public IReadOnlyDictionary<string, decimal?>? MarketCap { get; init; }
    /// <summary>24h trading volume (native + USD).</summary>
    [JsonPropertyName("volume_24h")] public IReadOnlyDictionary<string, decimal?>? Volume24h { get; init; }
    /// <summary>Floor-price 24h change percentage (native).</summary>
    [JsonPropertyName("floor_price_24h_percentage_change")] public IReadOnlyDictionary<string, decimal?>? FloorPrice24hPercentageChange { get; init; }
    /// <summary>Total supply.</summary>
    [JsonPropertyName("total_supply")] public decimal? TotalSupply { get; init; }
    /// <summary>Holders count.</summary>
    [JsonPropertyName("number_of_unique_addresses")] public decimal? NumberOfUniqueAddresses { get; init; }
    /// <summary>24h change in holder count.</summary>
    [JsonPropertyName("number_of_unique_addresses_24h_percentage_change")] public decimal? NumberOfUniqueAddresses24hPercentageChange { get; init; }
    /// <summary>Image set.</summary>
    [JsonPropertyName("image")] public NftImage? Image { get; init; }
    /// <summary>Banner image URL.</summary>
    [JsonPropertyName("banner_image")] public NftImage? BannerImage { get; init; }
    /// <summary>Links to external sites.</summary>
    [JsonPropertyName("links")] public NftLinks? Links { get; init; }
}

/// <summary>NFT paged-markets row.</summary>
public sealed class NftMarket : Nft
{
    /// <summary>UNIX seconds since last update.</summary>
    [JsonPropertyName("last_updated_at")] public string? LastUpdatedAt { get; init; }
}

/// <summary>NFT image variants.</summary>
public sealed class NftImage
{
    /// <summary>Small size.</summary>
    [JsonPropertyName("small")] public string? Small { get; init; }
    /// <summary>Small 2x size.</summary>
    [JsonPropertyName("small_2x")] public string? Small2x { get; init; }
}

/// <summary>NFT external links.</summary>
public sealed class NftLinks
{
    /// <summary>Home URL.</summary>
    [JsonPropertyName("homepage")] public string? Homepage { get; init; }
    /// <summary>Twitter URL.</summary>
    [JsonPropertyName("twitter")] public string? Twitter { get; init; }
    /// <summary>Discord URL.</summary>
    [JsonPropertyName("discord")] public string? Discord { get; init; }
}

/// <summary>One NFT marketplace listing.</summary>
public sealed class NftTicker
{
    /// <summary>Floor price in ETH.</summary>
    [JsonPropertyName("floor_price_in_native_currency")] public decimal? FloorPriceInNativeCurrency { get; init; }
    /// <summary>Native currency code.</summary>
    [JsonPropertyName("native_currency")] public string? NativeCurrency { get; init; }
    /// <summary>Last updated ISO-8601.</summary>
    [JsonPropertyName("updated_at")] public string? UpdatedAt { get; init; }
    /// <summary>NFT marketplace.</summary>
    [JsonPropertyName("nft_marketplace_id")] public string? NftMarketplaceId { get; init; }
    /// <summary>Marketplace name.</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>Marketplace home URL.</summary>
    [JsonPropertyName("nft_collection_url")] public string? NftCollectionUrl { get; init; }
}

/// <summary>Paged NFT-market options.</summary>
public sealed record NftMarketsOptions
{
    /// <summary>Filter to one chain.</summary>
    public string? AssetPlatformId { get; init; }
    /// <summary>Sort order.</summary>
    public string? Order { get; init; }
    /// <summary>Items per page.</summary>
    public int PerPage { get; init; } = 100;
    /// <summary>Page number.</summary>
    public int Page { get; init; } = 1;
}

/// <summary>NFT tickers wrapper.</summary>
public sealed class NftTickers
{
    /// <summary>Ticker list.</summary>
    [JsonPropertyName("tickers")] public IReadOnlyList<NftTicker> Tickers { get; init; } = Array.Empty<NftTicker>();
}

/// <summary>NFT market-chart point (timestamp + native-currency price).</summary>
public sealed record NftMarketChartPoint
{
    /// <summary>UTC timestamp.</summary>
    public DateTimeOffset Timestamp { get; init; }
    /// <summary>Floor price in native currency.</summary>
    public decimal? FloorPriceNative { get; init; }
    /// <summary>Floor price in USD.</summary>
    public decimal? FloorPriceUsd { get; init; }
    /// <summary>Market cap in native currency.</summary>
    public decimal? MarketCapNative { get; init; }
    /// <summary>24h volume in native currency.</summary>
    public decimal? Volume24hNative { get; init; }
}
