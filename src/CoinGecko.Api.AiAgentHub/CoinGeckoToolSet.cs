namespace CoinGecko.Api.AiAgentHub;

/// <summary>Flags enum selecting which tool groups <see cref="CoinGeckoAiTools.Create"/> returns.</summary>
[Flags]
public enum CoinGeckoToolSet
{
    /// <summary>No tools.</summary>
    None        = 0,
    /// <summary>Coin price lookups (current + historical).</summary>
    CoinPrices  = 1 << 0,
    /// <summary>Coin / NFT / exchange search.</summary>
    CoinSearch  = 1 << 1,
    /// <summary>Market-cap listings (top N by market cap).</summary>
    MarketData  = 1 << 2,
    /// <summary>Trending coins / NFTs / categories.</summary>
    Trending    = 1 << 3,
    /// <summary>Coin categories.</summary>
    Categories  = 1 << 4,
    /// <summary>NFT collections.</summary>
    Nfts        = 1 << 5,
    /// <summary>Derivatives (futures + perpetuals).</summary>
    Derivatives = 1 << 6,
    /// <summary>On-chain DEX / GeckoTerminal data.</summary>
    Onchain     = 1 << 7,
    /// <summary>All tool groups.</summary>
    All         = CoinPrices | CoinSearch | MarketData | Trending | Categories | Nfts | Derivatives | Onchain,
}
