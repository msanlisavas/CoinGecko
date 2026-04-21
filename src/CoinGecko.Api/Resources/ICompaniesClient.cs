using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Companies endpoints.</summary>
public interface ICompaniesClient
{
    /// <summary>Calls <c>GET /companies/public_treasury/{coin_id}</c>. Returns aggregated public treasury holdings for a coin.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<CompanyTreasury> GetPublicTreasuryAsync(string coinId, CancellationToken ct = default);
}
