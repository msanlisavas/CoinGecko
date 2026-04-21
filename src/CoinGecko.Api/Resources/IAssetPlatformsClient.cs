using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Asset Platforms endpoints.</summary>
public interface IAssetPlatformsClient
{
    /// <summary>Calls <c>GET /asset_platforms</c>. Returns all supported asset platforms, optionally filtered.</summary>
    Task<IReadOnlyList<AssetPlatform>> GetListAsync(string? filter = null, CancellationToken ct = default);

    /// <summary>Calls <c>GET /token_lists/{asset_platform_id}/all.json</c>. Returns the Uniswap-compatible token list for the given platform.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<TokenList> GetTokenListAsync(string assetPlatformId, CancellationToken ct = default);
}
