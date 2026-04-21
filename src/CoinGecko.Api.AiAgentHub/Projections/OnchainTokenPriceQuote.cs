namespace CoinGecko.Api.AiAgentHub.Projections;

/// <summary>Compact on-chain token price quote for LLM consumption.</summary>
/// <param name="NetworkId">Network/chain id the token belongs to (e.g. <c>"eth"</c>).</param>
/// <param name="Address">Contract address of the token.</param>
/// <param name="PriceUsd">Current token price in USD.</param>
public sealed record OnchainTokenPriceQuote(
    string NetworkId,
    string Address,
    decimal PriceUsd);
