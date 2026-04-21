namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact NFT collection snapshot for LLM consumption.</summary>
/// <param name="Id">CoinGecko NFT collection id.</param>
/// <param name="Name">Display name of the collection.</param>
/// <param name="Symbol">Ticker symbol of the collection.</param>
/// <param name="AssetPlatformId">Blockchain platform id the NFTs live on (e.g. <c>"ethereum"</c>), if known.</param>
/// <param name="FloorPriceNative">Floor price in the collection's native currency.</param>
/// <param name="FloorPriceUsd">Floor price converted to USD.</param>
/// <param name="MarketCapUsd">Total market cap of the collection in USD.</param>
/// <param name="Holders">Number of unique holder addresses.</param>
public sealed record NftSummary(
    string Id,
    string Name,
    string Symbol,
    string? AssetPlatformId,
    decimal? FloorPriceNative,
    decimal? FloorPriceUsd,
    decimal? MarketCapUsd,
    long? Holders);
