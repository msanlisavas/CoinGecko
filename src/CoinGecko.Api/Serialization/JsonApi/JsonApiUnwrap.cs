using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace CoinGecko.Api.Serialization.JsonApi;

/// <summary>Helpers for unwrapping JSON:API envelopes used by onchain endpoints.</summary>
internal static class JsonApiUnwrap
{
    public static async Task<T> ReadDataAsync<T>(
        HttpContent content,
        JsonTypeInfo<JsonApiResponse<T>> envelopeInfo,
        CancellationToken ct) where T : class
    {
        var env = await content.ReadFromJsonAsync(envelopeInfo, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("CoinGecko onchain endpoint returned empty body.");
        return env.Data ?? throw new InvalidOperationException("JSON:API envelope had null data.");
    }
}
