using System.Text.Json.Serialization;
using CoinGecko.Api.Serialization.JsonApi;

namespace CoinGecko.Api.Serialization;

/// <summary>
/// Source-generated JSON context. Every public DTO type used by the library is
/// registered here with a <c>[JsonSerializable]</c> attribute; later tasks add one
/// <c>[JsonSerializable(typeof(NewDto))]</c> entry each. AOT- and trim-safe.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy   = JsonKnownNamingPolicy.SnakeCaseLower,
    NumberHandling         = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters             = new[] { typeof(UnixSecondsConverter) })]
[JsonSerializable(typeof(JsonApiResponse<JsonApiResource>))]
[JsonSerializable(typeof(JsonApiResponse<JsonApiResource[]>))]
[JsonSerializable(typeof(JsonApiResource))]
[JsonSerializable(typeof(JsonApiResource[]))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(string[]))]
internal sealed partial class CoinGeckoJsonContext : JsonSerializerContext
{
}
