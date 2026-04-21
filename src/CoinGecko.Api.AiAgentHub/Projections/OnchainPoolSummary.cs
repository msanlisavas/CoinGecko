namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact on-chain DEX liquidity pool row for LLM consumption.</summary>
/// <param name="Id">GeckoTerminal pool id.</param>
/// <param name="Name">Display name of the pool (e.g. <c>"WETH / USDC"</c>).</param>
/// <param name="Address">Pool contract address on-chain.</param>
/// <param name="NetworkId">Network/chain id the pool lives on (e.g. <c>"eth"</c>), if available.</param>
/// <param name="BaseTokenPriceUsd">Current price of the base token in USD.</param>
/// <param name="QuoteTokenPriceUsd">Current price of the quote token in USD.</param>
/// <param name="ReserveUsd">Total liquidity reserve value in USD.</param>
/// <param name="Volume24hUsd">24h trading volume for the pool in USD.</param>
public sealed record OnchainPoolSummary(
    string Id,
    string Name,
    string Address,
    string? NetworkId,
    decimal? BaseTokenPriceUsd,
    decimal? QuoteTokenPriceUsd,
    decimal? ReserveUsd,
    decimal? Volume24hUsd);
