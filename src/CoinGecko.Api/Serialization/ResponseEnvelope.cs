namespace CoinGecko.Api.Serialization;

/// <summary>Selects which deserializer path to use for a given request.</summary>
public enum ResponseEnvelope
{
    /// <summary>Bare JSON object / array. Used for core endpoints.</summary>
    Bare = 0,

    /// <summary>JSON:API-style envelope. Used for onchain / GeckoTerminal endpoints.</summary>
    JsonApi = 1,
}
