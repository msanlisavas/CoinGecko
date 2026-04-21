using CoinGecko.Api.Models;

namespace CoinGecko.Api.Resources;

/// <summary>Sub-client for CoinGecko's /ping endpoint — API reachability + credentials probe.</summary>
public interface IPingClient
{
    /// <summary>Calls <c>GET /ping</c>. Returns the "gecko_says" welcome string.</summary>
    Task<PingResponse> PingAsync(CancellationToken ct = default);
}
