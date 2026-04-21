using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's Key endpoints.</summary>
public interface IKeyClient
{
    /// <summary>Calls <c>GET /key</c>. Returns current API key usage and rate-limit metadata.</summary>
    [RequiresPlan(CoinGeckoPlan.Basic)]
    Task<ApiKeyUsage> GetAsync(CancellationToken ct = default);
}
