namespace CoinGecko.Api.Serialization;

/// <summary>Per-request metadata attached via <see cref="System.Net.Http.HttpRequestMessage.Options"/>.</summary>
internal static class CoinGeckoRequestOptions
{
    /// <summary>Selects the envelope format to use when deserializing the response.</summary>
    public static readonly HttpRequestOptionsKey<ResponseEnvelope> Envelope = new("coingecko.envelope");

    /// <summary>Indicates the minimum plan required to call this endpoint.</summary>
    public static readonly HttpRequestOptionsKey<CoinGeckoPlan?>   RequiredPlan = new("coingecko.required_plan");

    /// <summary>Human-readable endpoint name used for logging and metrics.</summary>
    public static readonly HttpRequestOptionsKey<string>           EndpointName = new("coingecko.endpoint");

    /// <summary>Per-request correlation identifier.</summary>
    public static readonly HttpRequestOptionsKey<Guid>             RequestId = new("coingecko.request_id");
}
