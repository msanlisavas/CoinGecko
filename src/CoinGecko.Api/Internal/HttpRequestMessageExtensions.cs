using CoinGecko.Api.Serialization;

namespace CoinGecko.Api.Internal;

internal static class HttpRequestMessageExtensions
{
    public static ResponseEnvelope GetEnvelope(this HttpRequestMessage req)
        => req.Options.TryGetValue(CoinGeckoRequestOptions.Envelope, out var v) ? v : ResponseEnvelope.Bare;

    public static CoinGeckoPlan? GetRequiredPlan(this HttpRequestMessage req)
        => req.Options.TryGetValue(CoinGeckoRequestOptions.RequiredPlan, out var v) ? v : null;

    public static Guid GetOrCreateRequestId(this HttpRequestMessage req)
    {
        if (req.Options.TryGetValue(CoinGeckoRequestOptions.RequestId, out var id) && id != Guid.Empty)
        {
            return id;
        }

        var newId = Guid.NewGuid();
        req.Options.Set(CoinGeckoRequestOptions.RequestId, newId);
        return newId;
    }
}
